using System.ComponentModel;
using System.Net;
using WakeNetServer.Domain;
using WakeNetServer.Networking;
using WakeNetServer.Services;

namespace WakeNetServer.UI;

public sealed class MainForm : Form
{
    private static readonly TimeSpan OnlineThreshold = TimeSpan.FromSeconds(30);

    private sealed class ClientRow
    {
        public string ClientId { get; set; } = "";
        public string Hostname { get; set; } = "";
        public string Ip { get; set; } = "";
        public string Mac { get; set; } = "";
        public string Os { get; set; } = "";
        public DateTime LastSeenUtc { get; set; }

        public bool Online => DateTime.UtcNow - LastSeenUtc <= OnlineThreshold;
        public DateTime LastSeenLocal => LastSeenUtc.ToLocalTime();
    }

    private readonly TcpJsonServer _server = new();
    private readonly BindingList<ClientRow> _rows = new();

    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false
    };

    private readonly System.Windows.Forms.Timer _uiTimer = new();

    private readonly TextBox _txtPort = new() { Text = "5050", Width = 90 };
    private readonly Button _btnStart = new() { Text = "Start Listener", AutoSize = true };
    private readonly Button _btnStop = new() { Text = "Stop", AutoSize = true, Enabled = false };
    private readonly Button _btnShutdown = new() { Text = "Shutdown", AutoSize = true };
    private readonly Button _btnRestart = new() { Text = "Restart", AutoSize = true };
    private readonly Button _btnWol = new() { Text = "WOL", AutoSize = true };
    private readonly Label _lblStatus = new() { AutoSize = true };

    public MainForm(User user)
    {
        Text = "WakeNet";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(980, 520);

        var lbl = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(16),
            Text = $"Xin chào, {user.DisplayName} (@{user.Username})."
        };

        var btnLogout = new Button
        {
            Text = "Đăng xuất",
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(16)
        };
        btnLogout.Click += (_, _) => Close();

        ConfigureGrid();
        _grid.DataSource = _rows;

        _btnStart.Click += async (_, _) => await StartListenerAsync();
        _btnStop.Click += async (_, _) => await StopListenerAsync();
        _btnShutdown.Click += async (_, _) => await SendCommandAsync("shutdown");
        _btnRestart.Click += async (_, _) => await SendCommandAsync("restart");
        _btnWol.Click += async (_, _) => await SendWolAsync();

        _server.ClientUpserted += session => BeginInvoke(new Action(() => UpsertRow(session)));
        _server.ClientRemoved += clientId => BeginInvoke(new Action(() => RemoveRow(clientId)));

        FormClosing += (_, _) => { _ = StopListenerAsync(); };

        _uiTimer.Interval = 1000;
        _uiTimer.Tick += (_, _) =>
        {
            if (_rows.Count > 0)
            {
                // refresh Online/LastSeenLocal computed properties
                _grid.Refresh();
            }
        };
        _uiTimer.Start();

        var topBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(16),
            WrapContents = true,
            AutoSize = true
        };
        topBar.Controls.Add(new Label { Text = "Port:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        topBar.Controls.Add(_txtPort);
        topBar.Controls.Add(_btnStart);
        topBar.Controls.Add(_btnStop);
        topBar.Controls.Add(_btnShutdown);
        topBar.Controls.Add(_btnRestart);
        topBar.Controls.Add(_btnWol);
        topBar.Controls.Add(new Label { Text = "  ", AutoSize = true });
        topBar.Controls.Add(_lblStatus);

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0),
            ColumnCount = 1,
            RowCount = 4
        };
        panel.RowStyles.Clear();
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        panel.Controls.Add(lbl, 0, 0);
        panel.Controls.Add(topBar, 0, 1);
        panel.Controls.Add(_grid, 0, 2);
        panel.Controls.Add(btnLogout, 0, 3);

        Controls.Add(panel);
    }

    private void ConfigureGrid()
    {
        _grid.Columns.Clear();
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ClientRow.Hostname),
            HeaderText = "Tên máy",
            FillWeight = 120
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ClientRow.Ip),
            HeaderText = "IP",
            FillWeight = 80
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ClientRow.Mac),
            HeaderText = "MAC",
            FillWeight = 110
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ClientRow.Os),
            HeaderText = "OS",
            FillWeight = 140
        });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            DataPropertyName = nameof(ClientRow.Online),
            HeaderText = "Online",
            FillWeight = 50
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ClientRow.LastSeenLocal),
            HeaderText = "Last seen",
            FillWeight = 90,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "HH:mm:ss dd/MM" }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ClientRow.ClientId),
            HeaderText = "ClientId",
            FillWeight = 160
        });
    }

    private async Task StartListenerAsync()
    {
        if (_server.IsRunning) return;

        if (!int.TryParse(_txtPort.Text.Trim(), out var port) || port < 1 || port > 65535)
        {
            MessageBox.Show(this, "Port không hợp lệ.", "WakeNet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _rows.Clear();
        _server.Start(IPAddress.Any, port, clientTimeout: TimeSpan.FromSeconds(30));
        _lblStatus.Text = $"Listening 0.0.0.0:{port} (timeout 30s)";
        _btnStart.Enabled = false;
        _btnStop.Enabled = true;
        await Task.CompletedTask;
    }

    private async Task StopListenerAsync()
    {
        if (!_server.IsRunning) return;
        await _server.StopAsync();
        _lblStatus.Text = "Stopped";
        _btnStart.Enabled = true;
        _btnStop.Enabled = false;
        _rows.Clear();
    }

    private string? GetSelectedClientId()
    {
        if (_grid.SelectedRows.Count == 0) return null;
        var row = _grid.SelectedRows[0].DataBoundItem as ClientRow;
        return row?.ClientId;
    }

    private async Task SendCommandAsync(string action)
    {
        var clientId = GetSelectedClientId();
        if (clientId is null)
        {
            MessageBox.Show(this, "Vui lòng chọn 1 client.", "WakeNet", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            await _server.SendCommandAsync(clientId, action);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "WakeNet", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task SendWolAsync()
    {
        var clientId = GetSelectedClientId();
        if (clientId is null)
        {
            MessageBox.Show(this, "Vui lòng chọn 1 client.", "WakeNet", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var row = _rows.FirstOrDefault(r => r.ClientId == clientId);
        if (row is null || string.IsNullOrWhiteSpace(row.Mac))
        {
            MessageBox.Show(this, "Client không có MAC để gửi WOL.", "WakeNet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            await WakeOnLanService.SendMagicPacketAsync(row.Mac, port: 9);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "WakeNet", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpsertRow(ClientSession session)
    {
        var existing = _rows.FirstOrDefault(r => r.ClientId == session.ClientId);
        if (existing is null)
        {
            _rows.Add(new ClientRow
            {
                ClientId = session.ClientId,
                Hostname = session.Hostname,
                Ip = session.Ip,
                Mac = session.Mac,
                Os = session.Os,
                LastSeenUtc = session.LastSeenUtc
            });
        }
        else
        {
            existing.Hostname = session.Hostname;
            existing.Ip = session.Ip;
            existing.Mac = session.Mac;
            existing.Os = session.Os;
            existing.LastSeenUtc = session.LastSeenUtc;

            // Force grid refresh for BindingList objects.
            var idx = _rows.IndexOf(existing);
            _rows.ResetItem(idx);
        }
    }

    private void RemoveRow(string clientId)
    {
        var existing = _rows.FirstOrDefault(r => r.ClientId == clientId);
        if (existing is null) return;
        _rows.Remove(existing);
    }
}


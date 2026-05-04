using WakeNetServer.Services;

namespace WakeNetServer.UI;

public sealed class RegisterForm : Form
{
    private readonly AuthService _auth;

    private readonly TextBox _txtUsername = new() { PlaceholderText = "Username (>= 3 ký tự)" };
    private readonly TextBox _txtDisplayName = new() { PlaceholderText = "Display name" };
    private readonly TextBox _txtPassword = new() { PlaceholderText = "Password (>= 6 ký tự)", UseSystemPasswordChar = true };
    private readonly TextBox _txtConfirm = new() { PlaceholderText = "Nhập lại password", UseSystemPasswordChar = true };

    private readonly Button _btnRegister = new() { Text = "Đăng ký", AutoSize = true };
    private readonly Label _lblStatus = new() { AutoSize = true };

    public RegisterForm(AuthService auth)
    {
        _auth = auth;

        Text = "WakeNet - Đăng ký";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(460, 320);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 8,
            AutoSize = true,
        };

        layout.RowStyles.Clear();
        for (var i = 0; i < layout.RowCount; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Tạo tài khoản",
            Font = new Font(Font, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8)
        };

        _lblStatus.ForeColor = Color.Firebrick;

        _btnRegister.Click += async (_, _) => await RegisterAsync();
        AcceptButton = _btnRegister;

        layout.Controls.Add(title);
        layout.Controls.Add(_txtUsername);
        layout.Controls.Add(_txtDisplayName);
        layout.Controls.Add(_txtPassword);
        layout.Controls.Add(_txtConfirm);
        layout.Controls.Add(_btnRegister);
        layout.Controls.Add(_lblStatus);

        Controls.Add(layout);
    }

    private async Task RegisterAsync()
    {
        if (_txtPassword.Text != _txtConfirm.Text)
        {
            _lblStatus.Text = "Password xác nhận không khớp.";
            return;
        }

        SetBusy(true);
        try
        {
            _lblStatus.Text = string.Empty;
            await _auth.RegisterAsync(_txtUsername.Text, _txtDisplayName.Text, _txtPassword.Text);
            MessageBox.Show(this, "Đăng ký thành công. Bạn có thể đăng nhập.", "WakeNet", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _lblStatus.Text = ex.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        _btnRegister.Enabled = !busy;
        _txtUsername.Enabled = !busy;
        _txtDisplayName.Enabled = !busy;
        _txtPassword.Enabled = !busy;
        _txtConfirm.Enabled = !busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }
}


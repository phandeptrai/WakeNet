using WakeNetServer.Domain;
using WakeNetServer.Repositories;
using WakeNetServer.Services;

namespace WakeNetServer.UI;

public sealed class LoginForm : Form
{
    private readonly AuthService _auth;
    private readonly IClientRepository _clients;

    private readonly TextBox _txtUsername = new() { PlaceholderText = "Username" };
    private readonly TextBox _txtPassword = new() { PlaceholderText = "Password", UseSystemPasswordChar = true };
    private readonly Button _btnLogin = new() { Text = "Đăng nhập", AutoSize = true };
    private readonly LinkLabel _lnkRegister = new() { Text = "Chưa có tài khoản? Đăng ký" };
    private readonly Label _lblStatus = new() { AutoSize = true };

    public LoginForm(AuthService auth, IClientRepository clients)
    {
        _auth = auth;
        _clients = clients;

        Text = "WakeNet - Đăng nhập";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(420, 260);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 7,
            AutoSize = true,
        };

        layout.RowStyles.Clear();
        for (var i = 0; i < layout.RowCount; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Đăng nhập",
            Font = new Font(Font, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8)
        };

        _txtUsername.Dock = DockStyle.Top;
        _txtPassword.Dock = DockStyle.Top;
        _lblStatus.ForeColor = Color.Firebrick;

        _btnLogin.Click += async (_, _) => await LoginAsync();
        _lnkRegister.LinkClicked += (_, _) => OpenRegister();

        AcceptButton = _btnLogin;

        layout.Controls.Add(title);
        layout.Controls.Add(_txtUsername);
        layout.Controls.Add(_txtPassword);
        layout.Controls.Add(_btnLogin);
        layout.Controls.Add(_lnkRegister);
        layout.Controls.Add(_lblStatus);

        Controls.Add(layout);
    }

    private async Task LoginAsync()
    {
        SetBusy(true);
        try
        {
            _lblStatus.Text = string.Empty;
            var user = await _auth.LoginAsync(_txtUsername.Text, _txtPassword.Text);
            OpenMain(user);
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

    private void OpenRegister()
    {
        using var reg = new RegisterForm(_auth);
        reg.ShowDialog(this);
    }

    private void OpenMain(User user)
    {
        Hide();
        using var main = new MainForm(user, _clients);
        main.ShowDialog(this);
        Show();

        _txtPassword.Text = string.Empty;
        _txtPassword.Focus();
    }

    private void SetBusy(bool busy)
    {
        _btnLogin.Enabled = !busy;
        _lnkRegister.Enabled = !busy;
        _txtUsername.Enabled = !busy;
        _txtPassword.Enabled = !busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }
}


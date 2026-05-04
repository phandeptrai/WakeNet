using WakeNetServer.Domain;
using WakeNetServer.Repositories;

namespace WakeNetServer.Services;

public sealed class AuthService
{
    private readonly IUserRepository _users;

    public AuthService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<User> RegisterAsync(string username, string displayName, string password, CancellationToken ct = default)
    {
        username = (username ?? string.Empty).Trim();
        displayName = (displayName ?? string.Empty).Trim();

        if (username.Length < 3) throw new ArgumentException("Username tối thiểu 3 ký tự.");
        if (displayName.Length < 1) throw new ArgumentException("Display name không được rỗng.");
        if (password.Length < 6) throw new ArgumentException("Mật khẩu tối thiểu 6 ký tự.");

        var existing = await _users.FindByUsernameAsync(username, ct);
        if (existing is not null) throw new InvalidOperationException("Username đã tồn tại.");

        var (hash, salt, iterations) = PasswordHasher.Hash(password);
        return await _users.CreateAsync(username, displayName, hash, salt, iterations, ct);
    }

    public async Task<User> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        username = (username ?? string.Empty).Trim();
        if (username.Length == 0) throw new ArgumentException("Vui lòng nhập username.");

        var user = await _users.FindByUsernameAsync(username, ct);
        if (user is null) throw new InvalidOperationException("Sai username hoặc mật khẩu.");

        var ok = PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt, user.PasswordIterations);
        if (!ok) throw new InvalidOperationException("Sai username hoặc mật khẩu.");

        return user;
    }
}


using WakeNetServer.Domain;

namespace WakeNetServer.Repositories;

public interface IUserRepository
{
    Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default);
    Task<User> CreateAsync(
        string username,
        string displayName,
        string passwordHash,
        string passwordSalt,
        int passwordIterations,
        CancellationToken ct = default);
}


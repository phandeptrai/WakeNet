namespace WakeNetServer.Domain;

public sealed record User(
    long Id,
    string Username,
    string DisplayName,
    string PasswordHash,
    string PasswordSalt,
    int PasswordIterations,
    DateTime CreatedAtUtc);


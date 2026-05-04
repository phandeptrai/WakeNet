using Microsoft.Data.Sqlite;
using WakeNetServer.Data;
using WakeNetServer.Domain;

namespace WakeNetServer.Repositories;

public sealed class SqliteUserRepository : IUserRepository
{
    private readonly AppDb _db;

    public SqliteUserRepository(AppDb db)
    {
        _db = db;
    }

    public async Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default)
    {
        await using var conn = _db.OpenConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText =
            """
            SELECT Id, Username, DisplayName, PasswordHash, PasswordSalt, PasswordIterations, CreatedAtUtc
            FROM Users
            WHERE Username = $username
            LIMIT 1;
            """;
        cmd.Parameters.AddWithValue("$username", username);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        return new User(
            Id: reader.GetInt64(0),
            Username: reader.GetString(1),
            DisplayName: reader.GetString(2),
            PasswordHash: reader.GetString(3),
            PasswordSalt: reader.GetString(4),
            PasswordIterations: reader.GetInt32(5),
            CreatedAtUtc: DateTime.Parse(reader.GetString(6), null, System.Globalization.DateTimeStyles.RoundtripKind));
    }

    public async Task<User> CreateAsync(
        string username,
        string displayName,
        string passwordHash,
        string passwordSalt,
        int passwordIterations,
        CancellationToken ct = default)
    {
        await using var conn = _db.OpenConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText =
            """
            INSERT INTO Users (Username, DisplayName, PasswordHash, PasswordSalt, PasswordIterations, CreatedAtUtc)
            VALUES ($username, $displayName, $passwordHash, $passwordSalt, $passwordIterations, $createdAtUtc);

            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("$username", username);
        cmd.Parameters.AddWithValue("$displayName", displayName);
        cmd.Parameters.AddWithValue("$passwordHash", passwordHash);
        cmd.Parameters.AddWithValue("$passwordSalt", passwordSalt);
        cmd.Parameters.AddWithValue("$passwordIterations", passwordIterations);
        cmd.Parameters.AddWithValue("$createdAtUtc", DateTime.UtcNow.ToString("O"));

        try
        {
            var idObj = await cmd.ExecuteScalarAsync(ct);
            var id = Convert.ToInt64(idObj);
            return (await FindByUsernameAsync(username, ct))! with { Id = id };
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // constraint violation
        {
            throw new InvalidOperationException("Username đã tồn tại.", ex);
        }
    }
}


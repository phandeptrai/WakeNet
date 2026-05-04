using Microsoft.Data.Sqlite;

namespace WakeNetServer.Data;

public sealed class AppDb : IDisposable
{
    private readonly string _dbPath;

    public AppDb(string? dbPath = null)
    {
        _dbPath = dbPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WakeNet",
            "WakeNet.db");
    }

    public SqliteConnection OpenConnection()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
        var conn = new SqliteConnection($"Data Source={_dbPath};Cache=Shared;");
        conn.Open();
        return conn;
    }

    public void Initialize()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            """
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                DisplayName TEXT NOT NULL,
                PasswordHash TEXT NOT NULL,
                PasswordSalt TEXT NOT NULL,
                PasswordIterations INTEGER NOT NULL,
                CreatedAtUtc TEXT NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }

    public void Dispose()
    {
        // Connections are created per-operation; nothing to dispose here.
    }
}


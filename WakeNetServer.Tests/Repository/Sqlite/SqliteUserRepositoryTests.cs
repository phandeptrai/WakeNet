using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WakeNetServer.Server.Models;
using WakeNetServer.Server.Repository.Sqlite;
using Xunit;

namespace WakeNetServer.Tests.Repository.Sqlite;

public class SqliteUserRepositoryTests
{
    [Fact]
    public void Upsert_And_GetByUsername_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();

        var opts = new DbContextOptionsBuilder<WakeNetDbContext>()
            .UseSqlite(conn)
            .Options;

        using var db = new WakeNetDbContext(opts);
        db.Database.EnsureCreated();

        var repo = new SqliteUserRepository(db);

        var u = new User
        {
            Id = "u1",
            Username = "admin",
            PasswordHash = "hash"
        };

        repo.Upsert(u);

        var loaded = repo.GetByUsername("admin");
        Assert.NotNull(loaded);
        Assert.Equal("u1", loaded!.Id);
    }
}


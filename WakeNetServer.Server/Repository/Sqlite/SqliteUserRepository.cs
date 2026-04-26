using Microsoft.EntityFrameworkCore;
using WakeNetServer.Server.Models;
using WakeNetServer.Server.Repository.Interfaces;

namespace WakeNetServer.Server.Repository.Sqlite;

public sealed class SqliteUserRepository : IUserRepository
{
    private readonly WakeNetDbContext _db;

    public SqliteUserRepository(WakeNetDbContext db) => _db = db;

    public User? GetById(string id) =>
        _db.Users.AsNoTracking().FirstOrDefault(x => x.Id == id);

    public User? GetByUsername(string username) =>
        _db.Users.AsNoTracking().FirstOrDefault(x => x.Username == username);

    public List<User> GetAll() =>
        _db.Users.AsNoTracking().OrderBy(x => x.Username).ToList();

    public void Upsert(User user)
    {
        var existing = _db.Users.FirstOrDefault(x => x.Id == user.Id);
        if (existing is null)
            _db.Users.Add(user);
        else
            _db.Entry(existing).CurrentValues.SetValues(user);

        _db.SaveChanges();
    }
}


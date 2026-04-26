using Microsoft.EntityFrameworkCore;
using WakeNetServer.Server.Models;
using WakeNetServer.Server.Repository.Interfaces;

namespace WakeNetServer.Server.Repository.Sqlite;

public sealed class SqliteMachineRepository : IMachineRepository
{
    private readonly WakeNetDbContext _db;

    public SqliteMachineRepository(WakeNetDbContext db) => _db = db;

    public Machine? GetById(string id) =>
        _db.Machines.AsNoTracking().FirstOrDefault(x => x.Id == id);

    public List<Machine> GetAll() =>
        _db.Machines.AsNoTracking().OrderBy(x => x.Id).ToList();

    public void Upsert(Machine machine)
    {
        var existing = _db.Machines.FirstOrDefault(x => x.Id == machine.Id);
        if (existing is null)
            _db.Machines.Add(machine);
        else
            _db.Entry(existing).CurrentValues.SetValues(machine);

        _db.SaveChanges();
    }

    public void UpdateLastSeen(string machineId, DateTime lastSeenUtc)
    {
        var existing = _db.Machines.FirstOrDefault(x => x.Id == machineId);
        if (existing is null) return;
        existing.LastSeen = DateTime.SpecifyKind(lastSeenUtc, DateTimeKind.Utc);
        _db.SaveChanges();
    }

    public void UpdateStatus(string machineId, MachineStatus status)
    {
        var existing = _db.Machines.FirstOrDefault(x => x.Id == machineId);
        if (existing is null) return;
        existing.Status = status;
        _db.SaveChanges();
    }
}


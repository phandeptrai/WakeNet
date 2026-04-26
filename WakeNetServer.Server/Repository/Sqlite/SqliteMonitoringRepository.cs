using Microsoft.EntityFrameworkCore;
using WakeNetServer.Server.Models;
using WakeNetServer.Server.Repository.Interfaces;

namespace WakeNetServer.Server.Repository.Sqlite;

public sealed class SqliteMonitoringRepository : IMonitoringRepository
{
    private readonly WakeNetDbContext _db;

    public SqliteMonitoringRepository(WakeNetDbContext db) => _db = db;

    public void SaveLatest(SystemInfo info)
    {
        var existing = _db.SystemInfos.FirstOrDefault(x => x.MachineId == info.MachineId);
        if (existing is null)
            _db.SystemInfos.Add(info);
        else
            _db.Entry(existing).CurrentValues.SetValues(info);

        _db.SaveChanges();
    }

    public SystemInfo? GetLatest(string machineId) =>
        _db.SystemInfos.AsNoTracking().FirstOrDefault(x => x.MachineId == machineId);
}


using Microsoft.EntityFrameworkCore;
using WakeNetServer.Server.Models;
using WakeNetServer.Server.Repository.Interfaces;

namespace WakeNetServer.Server.Repository.Sqlite;

public sealed class SqliteCommandRepository : ICommandRepository
{
    private readonly WakeNetDbContext _db;

    public SqliteCommandRepository(WakeNetDbContext db) => _db = db;

    public Command? GetById(string id) =>
        _db.Commands.AsNoTracking().FirstOrDefault(x => x.Id == id);

    public void Add(Command command)
    {
        _db.Commands.Add(command);
        _db.SaveChanges();
    }

    public List<Command> GetPendingByMachineId(string machineId)
    {
        return _db.Commands.AsNoTracking()
            .Where(x => x.MachineId == machineId && x.Status == CommandStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToList();
    }

    public void Update(Command command)
    {
        var existing = _db.Commands.FirstOrDefault(x => x.Id == command.Id);
        if (existing is null)
            _db.Commands.Add(command);
        else
            _db.Entry(existing).CurrentValues.SetValues(command);

        _db.SaveChanges();
    }

    public void SaveResult(CommandResult result)
    {
        var existing = _db.CommandResults.FirstOrDefault(x => x.CommandId == result.CommandId);
        if (existing is null)
            _db.CommandResults.Add(result);
        else
            _db.Entry(existing).CurrentValues.SetValues(result);

        _db.SaveChanges();
    }

    public CommandResult? GetResultByCommandId(string commandId) =>
        _db.CommandResults.AsNoTracking().FirstOrDefault(x => x.CommandId == commandId);
}


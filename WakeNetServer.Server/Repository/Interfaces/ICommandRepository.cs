// Lưu/đọc Command và CommandResult theo máy.
using WakeNetServer.Server.Models;

namespace WakeNetServer.Server.Repository.Interfaces;

public interface ICommandRepository
{
    Command? GetById(string id);

    void Add(Command command);

    List<Command> GetPendingByMachineId(string machineId);

    void Update(Command command);

    void SaveResult(CommandResult result);

    CommandResult? GetResultByCommandId(string commandId);
}


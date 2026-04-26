using WakeNetServer.Server.Models;

namespace WakeNetServer.Server.Services.Interfaces;

public interface ICommandService
{
    void CreateCommand(Command command);

    List<Command> GetPendingCommands(string machineId);

    void UpdateCommandResult(CommandResult result);
}


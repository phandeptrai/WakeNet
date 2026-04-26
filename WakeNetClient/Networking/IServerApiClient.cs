using System.Collections.Generic;
using WakeNetClient.Models;

namespace WakeNetClient.Networking
{
    public interface IServerApiClient
    {
        MachineDto RegisterOrUpdateMachine(MachineDto machine);
        List<CommandDto> GetPendingCommands(string machineId);
        void SendCommandResult(CommandResultDto result);
        void SendSystemInfo(SystemInfoDto info);
    }
}

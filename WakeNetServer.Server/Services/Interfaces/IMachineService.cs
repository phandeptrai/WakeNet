using WakeNetServer.Server.Models;

namespace WakeNetServer.Server.Services.Interfaces;

public interface IMachineService
{
    void RegisterMachine(Machine machine);

    List<Machine> GetAllMachines();

    void UpdateLastSeen(string machineId);

    void UpdateStatus(string machineId, MachineStatus status);
}


using WakeNetServer.Server.Models;

namespace WakeNetServer.Server.Services.Interfaces;

public interface IMonitoringService
{
    void SaveSystemInfo(SystemInfo info);

    SystemInfo GetLatest(string machineId);
}


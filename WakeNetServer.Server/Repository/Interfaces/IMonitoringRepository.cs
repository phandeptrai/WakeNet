// Lưu/đọc SystemInfo mới nhất của từng máy.
using WakeNetServer.Server.Models;

namespace WakeNetServer.Server.Repository.Interfaces;

public interface IMonitoringRepository
{
    void SaveLatest(SystemInfo info);
    SystemInfo? GetLatest(string machineId);
}


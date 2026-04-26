// Lưu/đọc danh sách Machine (trạng thái máy).
using WakeNetServer.Server.Models;

namespace WakeNetServer.Server.Repository.Interfaces;

public interface IMachineRepository
{
    Machine? GetById(string id);
    List<Machine> GetAll();

    /// <summary>
    /// Insert or update by Machine.Id
    /// </summary>
    void Upsert(Machine machine);

    void UpdateLastSeen(string machineId, DateTime lastSeenUtc);
    void UpdateStatus(string machineId, MachineStatus status);
}


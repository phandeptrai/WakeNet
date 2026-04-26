namespace WakeNetServer.Server.Models;

// Thông tin phần cứng máy client gửi lên
public class SystemInfo
{
    public string MachineId { get; set; } = string.Empty;

    public string Cpu { get; set; } = string.Empty;
    public int RamGB { get; set; }
    public double CpuUsage { get; set; }
    public double RamUsage { get; set; }
}


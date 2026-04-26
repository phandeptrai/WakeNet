namespace WakeNetServer.Server.Models;

// Kết quả thực thi từ client gửi về server
public class CommandResult
{
    public string CommandId { get; set; } = string.Empty;
    public string MachineId { get; set; } = string.Empty;

    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public DateTime ExecutedAt { get; set; }
}


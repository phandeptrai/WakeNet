namespace WakeNetServer.Server.Models;

public class Command
{
    public string Id { get; set; } = string.Empty;
    public string MachineId { get; set; } = string.Empty;

    public CommandType Type { get; set; }        // Shutdown, KillProcess...
    public string Payload { get; set; } = string.Empty;  // dữ liệu bổ sung

    public CommandStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}


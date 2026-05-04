namespace WakeNetServer.Protocol.Dtos;

public sealed class HeartbeatMessage
{
    public string? type { get; set; }
    public string? clientId { get; set; }
}


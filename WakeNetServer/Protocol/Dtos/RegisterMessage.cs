namespace WakeNetServer.Protocol.Dtos;

public sealed class RegisterMessage
{
    public string? type { get; set; }
    public string? clientId { get; set; }
    public string? hostname { get; set; }
    public string? ip { get; set; }
    public string? mac { get; set; }
    public string? os { get; set; }
}


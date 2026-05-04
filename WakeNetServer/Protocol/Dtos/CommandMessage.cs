namespace WakeNetServer.Protocol.Dtos;

public sealed class CommandMessage
{
    public string type { get; set; } = "command";
    public string action { get; set; } = "";
}


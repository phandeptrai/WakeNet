using System.Net.Sockets;

namespace WakeNetServer.Networking;

public sealed class ClientSession
{
    public required string ClientId { get; init; }
    public required string Hostname { get; set; }
    public required string Ip { get; set; }
    public required string Mac { get; set; }
    public required string Os { get; set; }

    public required TcpClient TcpClient { get; set; }
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;

    public string RemoteEndpoint => TcpClient.Client.RemoteEndPoint?.ToString() ?? "";
}


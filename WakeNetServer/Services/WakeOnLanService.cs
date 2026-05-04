using System.Net;
using System.Net.Sockets;

namespace WakeNetServer.Services;

public static class WakeOnLanService
{
    public static async Task SendMagicPacketAsync(string macAddress, int port = 9, CancellationToken ct = default)
    {
        var macBytes = ParseMac(macAddress);
        var packet = BuildMagicPacket(macBytes);

        using var udp = new UdpClient();
        udp.EnableBroadcast = true;

        var endpoint = new IPEndPoint(IPAddress.Broadcast, port);
        await udp.SendAsync(packet, packet.Length, endpoint).WaitAsync(ct);
    }

    private static byte[] ParseMac(string mac)
    {
        mac = (mac ?? string.Empty).Trim();
        mac = mac.Replace("-", "").Replace(":", "").Replace(".", "");
        if (mac.Length != 12) throw new ArgumentException("MAC address không hợp lệ.");

        var bytes = new byte[6];
        for (var i = 0; i < 6; i++)
        {
            bytes[i] = Convert.ToByte(mac.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    private static byte[] BuildMagicPacket(byte[] mac)
    {
        var packet = new byte[6 + 16 * 6];
        for (var i = 0; i < 6; i++) packet[i] = 0xFF;
        for (var i = 6; i < packet.Length; i += 6)
        {
            Buffer.BlockCopy(mac, 0, packet, i, 6);
        }
        return packet;
    }
}


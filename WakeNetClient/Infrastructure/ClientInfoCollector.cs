using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace WakeNetClient.Infrastructure
{
    internal sealed class ClientInfo
    {
        public string ClientId { get; set; }
        public string Hostname { get; set; }
        public string Ip { get; set; }
        public string Mac { get; set; }
        public string Os { get; set; }
    }

    internal static class ClientInfoCollector
    {
        public static ClientInfo Collect(string clientId)
        {
            var host = Environment.MachineName;
            var ip = GetLocalIpv4() ?? "0.0.0.0";
            var mac = GetPrimaryMac() ?? "00:00:00:00:00:00";
            var os = GetOsString();

            return new ClientInfo
            {
                ClientId = clientId,
                Hostname = host,
                Ip = ip,
                Mac = mac,
                Os = os
            };
        }

        private static string GetOsString()
        {
            // Example: Microsoft Windows 10.0.19045
            return RuntimeInformation.OSDescription?.Trim() ?? Environment.OSVersion.ToString();
        }

        private static string GetLocalIpv4()
        {
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                    var props = ni.GetIPProperties();
                    var addr = props.UnicastAddresses
                        .Select(a => a.Address)
                        .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                                             !IPAddress.IsLoopback(a));
                    if (addr != null) return addr.ToString();
                }
            }
            catch { }

            return null;
        }

        private static string GetPrimaryMac()
        {
            try
            {
                var best = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up)
                    .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .OrderByDescending(n => n.Speed)
                    .FirstOrDefault();

                var bytes = best != null ? best.GetPhysicalAddress().GetAddressBytes() : null;
                if (bytes == null || bytes.Length != 6) return null;
                return string.Join(":", bytes.Select(b => b.ToString("X2")));
            }
            catch { }

            return null;
        }
    }
}


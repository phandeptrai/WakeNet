using System;
using System.Configuration;

namespace WakeNetClient.Config
{
    internal sealed class ClientSettings
    {
        public string ServerHost { get; }
        public int ServerPort { get; }
        public bool HeartbeatEnabled { get; }
        public TimeSpan HeartbeatInterval { get; }
        public TimeSpan ReconnectDelay { get; }

        private ClientSettings(
            string serverHost,
            int serverPort,
            bool heartbeatEnabled,
            TimeSpan heartbeatInterval,
            TimeSpan reconnectDelay)
        {
            ServerHost = serverHost;
            ServerPort = serverPort;
            HeartbeatEnabled = heartbeatEnabled;
            HeartbeatInterval = heartbeatInterval;
            ReconnectDelay = reconnectDelay;
        }

        public static ClientSettings Load()
        {
            var host = (ConfigurationManager.AppSettings["ServerHost"] ?? "127.0.0.1").Trim();
            var portStr = (ConfigurationManager.AppSettings["ServerPort"] ?? "5050").Trim();
            var hbEnabledStr = (ConfigurationManager.AppSettings["HeartbeatEnabled"] ?? "true").Trim();
            var hbIntervalStr = (ConfigurationManager.AppSettings["HeartbeatIntervalSeconds"] ?? "10").Trim();
            var reconnectDelayStr = (ConfigurationManager.AppSettings["ReconnectDelaySeconds"] ?? "5").Trim();

            int port;
            if (!int.TryParse(portStr, out port) || port < 1 || port > 65535) port = 5050;

            bool hbEnabled;
            if (!bool.TryParse(hbEnabledStr, out hbEnabled)) hbEnabled = true;

            int hbSeconds;
            if (!int.TryParse(hbIntervalStr, out hbSeconds) || hbSeconds < 1) hbSeconds = 10;

            int reconnectSeconds;
            if (!int.TryParse(reconnectDelayStr, out reconnectSeconds) || reconnectSeconds < 1) reconnectSeconds = 5;

            return new ClientSettings(
                serverHost: host,
                serverPort: port,
                heartbeatEnabled: hbEnabled,
                heartbeatInterval: TimeSpan.FromSeconds(hbSeconds),
                reconnectDelay: TimeSpan.FromSeconds(reconnectSeconds));
        }
    }
}


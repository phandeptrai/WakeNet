namespace WakeNetClient.Protocol.Dtos
{
    internal sealed class HeartbeatMessage
    {
        public string type { get; set; } = "heartbeat";
        public string clientId { get; set; }
    }
}


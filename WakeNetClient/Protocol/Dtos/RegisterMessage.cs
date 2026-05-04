namespace WakeNetClient.Protocol.Dtos
{
    internal sealed class RegisterMessage
    {
        public string type { get; set; } = "register";
        public string clientId { get; set; }
        public string hostname { get; set; }
        public string ip { get; set; }
        public string mac { get; set; }
        public string os { get; set; }
    }
}


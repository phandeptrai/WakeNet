using WakeNetClient.Enums;

namespace WakeNetClient.Models
{
    public class MachineDto
    {
        public string Id { get; set; }
        public string HostName { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public MachineStatus Status { get; set; }
        public System.DateTime LastSeen { get; set; }
    }
}

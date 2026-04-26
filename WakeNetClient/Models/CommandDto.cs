using WakeNetClient.Enums;

namespace WakeNetClient.Models
{
    public class CommandDto
    {
        public string Id { get; set; }
        public string MachineId { get; set; }
        public CommandType Type { get; set; }
        public string Payload { get; set; }
        public CommandStatus Status { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}

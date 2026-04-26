namespace WakeNetClient.Models
{
    public class CommandResultDto
    {
        public string CommandId { get; set; }
        public string MachineId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public System.DateTime ExecutedAt { get; set; }
    }
}

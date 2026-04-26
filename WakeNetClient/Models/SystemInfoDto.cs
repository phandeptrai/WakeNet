namespace WakeNetClient.Models
{
    public class SystemInfoDto
    {
        public string MachineId { get; set; }
        public string Cpu { get; set; }
        public int RamGB { get; set; }
        public double CpuUsage { get; set; }
        public double RamUsage { get; set; }
    }
}

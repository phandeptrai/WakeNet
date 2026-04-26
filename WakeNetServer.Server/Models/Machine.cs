namespace WakeNetServer.Server.Models;

public class Machine
{
    public string Id { get; set; } = string.Empty;           // ID duy nhất của máy
    public string HostName { get; set; } = string.Empty;     // tên máy
    public string IpAddress { get; set; } = string.Empty;    // IP
    public string MacAddress { get; set; } = string.Empty;   // MAC

    public MachineStatus Status { get; set; }                // Online/Offline/Sleeping
    public DateTime LastSeen { get; set; }                   // lần cuối ping server
}


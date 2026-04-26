namespace WakeNetServer.Server.Models;

public enum MachineStatus
{
    Offline,
    Online,
    Sleeping
}

public enum CommandType
{
    Shutdown,
    Restart,
    WakeOnLan,
    KillProcess,
    BlockApp
}

public enum CommandStatus
{
    Pending,
    Sent,
    Executed,
    Failed
}


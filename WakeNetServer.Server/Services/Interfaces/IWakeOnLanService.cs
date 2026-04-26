namespace WakeNetServer.Server.Services.Interfaces;

public interface IWakeOnLanService
{
    void SendMagicPacket(string macAddress);
}


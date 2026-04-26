using WakeNetClient.Models;

namespace WakeNetClient.Services.Interfaces
{
    public interface IMonitoringSender
    {
        void Send(SystemInfoDto info);
    }
}

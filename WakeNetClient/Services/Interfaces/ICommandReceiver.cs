using System.Collections.Generic;
using WakeNetClient.Models;

namespace WakeNetClient.Services.Interfaces
{
    public interface ICommandReceiver
    {
        List<CommandDto> FetchPending(string machineId);
    }
}

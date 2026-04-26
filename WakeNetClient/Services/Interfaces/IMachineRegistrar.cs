using WakeNetClient.Models;

namespace WakeNetClient.Services.Interfaces
{
    public interface IMachineRegistrar
    {
        MachineDto RegisterOrUpdate(MachineDto machine);
    }
}

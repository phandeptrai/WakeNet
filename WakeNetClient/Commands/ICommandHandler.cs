using WakeNetClient.Models;

namespace WakeNetClient.Commands
{
    public interface ICommandHandler
    {
        bool CanHandle(CommandDto command);
        CommandResultDto Handle(CommandDto command);
    }
}

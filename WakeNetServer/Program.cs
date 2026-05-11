using WakeNetServer.Data;
using WakeNetServer.Repositories;
using WakeNetServer.Services;
using WakeNetServer.UI;

namespace WakeNetServer;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var db = new AppDb();
        db.Initialize();

        var userRepo = new SqliteUserRepository(db);
        var authService = new AuthService(userRepo);
        var clientRepo = new SqliteClientRepository(db);

        Application.Run(new LoginForm(authService, clientRepo));
    }
}

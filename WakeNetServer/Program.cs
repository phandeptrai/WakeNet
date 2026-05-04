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

        using var db = new AppDb();
        db.Initialize();

        var userRepo = new SqliteUserRepository(db);
        var authService = new AuthService(userRepo);

        Application.Run(new LoginForm(authService));
    }
}

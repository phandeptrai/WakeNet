using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WakeNetServer.Server.Repository.Interfaces;

namespace WakeNetServer.Server.Repository.Sqlite;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteRepositories(this IServiceCollection services)
    {
        // `DotEnv` đã normalize path tương đối thành absolute theo vị trí `.env`.
        var sqlitePath = Environment.GetEnvironmentVariable("WAKENET_SQLITE_PATH") ?? "./WakeNetData/wakenet.db";
        sqlitePath = Path.GetFullPath(sqlitePath);

        Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath) ?? Directory.GetCurrentDirectory());

        services.AddDbContext<WakeNetDbContext>(opt => opt.UseSqlite($"Data Source={sqlitePath}"));

        services.AddScoped<IMachineRepository, SqliteMachineRepository>();
        services.AddScoped<ICommandRepository, SqliteCommandRepository>();
        services.AddScoped<IMonitoringRepository, SqliteMonitoringRepository>();
        services.AddScoped<IUserRepository, SqliteUserRepository>();

        return services;
    }
}


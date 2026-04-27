using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WakeNetServer.Server.Repository.Interfaces;

namespace WakeNetServer.Server.Repository.Sqlite;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteRepositories(this IServiceCollection services, IConfiguration config)
    {
        var sqlitePath = config["WakeNet:SqlitePath"] ?? "../WakeNetData/wakenet.db";
        var contentRoot = Directory.GetCurrentDirectory();

        if (!Path.IsPathRooted(sqlitePath))
            sqlitePath = Path.GetFullPath(Path.Combine(contentRoot, sqlitePath));

        Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath) ?? contentRoot);

        services.AddDbContext<WakeNetDbContext>(opt => opt.UseSqlite($"Data Source={sqlitePath}"));

        services.AddScoped<IMachineRepository, SqliteMachineRepository>();
        services.AddScoped<ICommandRepository, SqliteCommandRepository>();
        services.AddScoped<IMonitoringRepository, SqliteMonitoringRepository>();
        services.AddScoped<IUserRepository, SqliteUserRepository>();

        return services;
    }
}


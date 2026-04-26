using Microsoft.EntityFrameworkCore;
using WakeNetServer.Server.Models;

namespace WakeNetServer.Server.Repository.Sqlite;

public sealed class WakeNetDbContext : DbContext
{
    public WakeNetDbContext(DbContextOptions<WakeNetDbContext> options) : base(options) { }

    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<Command> Commands => Set<Command>();
    public DbSet<CommandResult> CommandResults => Set<CommandResult>();
    public DbSet<SystemInfo> SystemInfos => Set<SystemInfo>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Machine>().HasKey(x => x.Id);
        modelBuilder.Entity<Command>().HasKey(x => x.Id);

        modelBuilder.Entity<CommandResult>().HasKey(x => x.CommandId);
        modelBuilder.Entity<CommandResult>().HasIndex(x => x.MachineId);

        // Lưu latest theo machineId (1 record / machine)
        modelBuilder.Entity<SystemInfo>().HasKey(x => x.MachineId);

        modelBuilder.Entity<User>().HasKey(x => x.Id);
        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();
    }
}


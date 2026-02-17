using Microsoft.EntityFrameworkCore;
using SimulatorApp.Core.Models;

namespace SimulatorApp.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<StateChangeLog> StateChangeLogs => Set<StateChangeLog>();
    public DbSet<StatusChangeLog> StatusChangeLogs => Set<StatusChangeLog>();
    public DbSet<TelemetryLog> TelemetryLogs => Set<TelemetryLog>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<AlertLog> AlertLogs => Set<AlertLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>()
            .HasKey(e => e.DeviceId);

        modelBuilder.Entity<Sensor>()
            .HasKey(e => e.SensorId);
        modelBuilder.Entity<Sensor>()
            .HasOne(e => e.Device)
            .WithMany(e => e.Sensors)
            .HasForeignKey(e => e.DeviceId);

        modelBuilder.Entity<StateChangeLog>()
            .HasKey(e => e.StateChangeLogId);
        modelBuilder.Entity<StateChangeLog>()
            .HasIndex(e => new { e.SensorId, e.Timestamp });
        modelBuilder.Entity<StateChangeLog>()
            .HasOne(e => e.Sensor)
            .WithMany(e => e.StateChanges)
            .HasForeignKey(e => e.SensorId);

        modelBuilder.Entity<StatusChangeLog>()
            .HasKey(e => e.StatusChangeLogId);
        modelBuilder.Entity<StatusChangeLog>()
            .HasIndex(e => new { e.SensorId, e.Timestamp });
        modelBuilder.Entity<StatusChangeLog>()
            .HasOne(e => e.Sensor)
            .WithMany(e => e.StatusChanges)
            .HasForeignKey(e => e.SensorId);

        modelBuilder.Entity<TelemetryLog>()
            .HasKey(e => e.TelemetryLogId);
        modelBuilder.Entity<TelemetryLog>()
            .HasIndex(e => new { e.SensorId, e.Timestamp });
        modelBuilder.Entity<TelemetryLog>()
            .HasOne(e => e.Sensor)
            .WithMany(e => e.Telemetries)
            .HasForeignKey(e => e.SensorId);

        modelBuilder.Entity<AlertRule>()
            .HasKey(e => e.AlertRuleId);
        modelBuilder.Entity<AlertRule>()
            .HasOne(e => e.Sensor)
            .WithMany()
            .HasForeignKey(e => e.SensorId);

        modelBuilder.Entity<AlertLog>()
            .HasKey(e => e.AlertLogId);
        modelBuilder.Entity<AlertLog>()
            .HasIndex(e => new { e.SensorId, e.Timestamp });
        modelBuilder.Entity<AlertLog>()
            .HasOne(e => e.AlertRule)
            .WithMany()
            .HasForeignKey(e => e.AlertRuleId);
        modelBuilder.Entity<AlertLog>()
            .HasOne(e => e.Sensor)
            .WithMany()
            .HasForeignKey(e => e.SensorId);
    }
}

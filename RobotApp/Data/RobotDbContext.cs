using Microsoft.EntityFrameworkCore;
using RobotApp.Models;
using RobotApp.Models.Measurements;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RobotApp.Data;

public class RobotDbContext : DbContext
{
    public RobotDbContext(DbContextOptions<RobotDbContext> options)
        : base(options) { }
    public DbSet<Robot> Robots => Set<Robot>();

    public DbSet<StateSnapshot> StateSnapshots => Set<StateSnapshot>();
    public DbSet<TemperatureMeasurement> TemperatureMeasurements => Set<TemperatureMeasurement>();
    public DbSet<HumidityMeasurement> HumidityMeasurements => Set<HumidityMeasurement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Robot>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<StateSnapshot>()
            .HasIndex(s => new { s.RobotName, s.Timestamp });

        modelBuilder.Entity<TemperatureMeasurement>()
            .HasIndex(t => new { t.RobotName, t.Timestamp });

        modelBuilder.Entity<HumidityMeasurement>()
            .HasIndex(h => new { h.RobotName, h.Timestamp });
    }
}

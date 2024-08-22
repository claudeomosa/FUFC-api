using FUFC.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FUFC.Shared.Data;

public class UfcContext : DbContext
{
    public UfcContext(DbContextOptions<UfcContext> options) : base(options)
    {
    }

    public DbSet<Gym> Gyms { get; set; }

    public DbSet<Fighter> Fighters { get; set; }

    public DbSet<Referee> Referees { get; set; }

    public DbSet<Event> Events { get; set; }

    public DbSet<Bout> Bouts { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Fighter>()
            .OwnsOne(fighter => fighter.Record, builder => { builder.ToJson(); })
            .OwnsOne(fighter => fighter.SkillStats, builder => builder.ToJson())
            .OwnsOne(fighter => fighter.SocialMedia, builder => builder.ToJson());
            
    }

}

public class UfcContextFactory : IDesignTimeDbContextFactory<UfcContext>
{
    public UfcContext CreateDbContext(string[] args)
    {
        // Build the configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Create options builder
        var optionsBuilder = new DbContextOptionsBuilder<UfcContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("UfcDB"));

        return new UfcContext(optionsBuilder.Options);
    }
}


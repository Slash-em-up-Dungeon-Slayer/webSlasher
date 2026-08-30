using DungeonSlayer.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace DungeonSlayer.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<PlayerProgress> PlayerProgresses { get; set; } = null!;
    public DbSet<RunResult> RunResults { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerProgress>()
            .HasKey(p => p.PlayerId);

        modelBuilder.Entity<Player>()
            .HasOne<PlayerProgress>()
            .WithOne()
            .HasForeignKey<PlayerProgress>(p => p.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RunResult>()
            .HasIndex(r => r.PlayerId);
    }
}

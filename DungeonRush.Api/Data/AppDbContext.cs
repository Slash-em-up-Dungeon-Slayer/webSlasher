using DungeonRush.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace DungeonRush.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>()
            .HasIndex(p => p.Email)
            .IsUnique();
    }
}

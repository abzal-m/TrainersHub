using Microsoft.EntityFrameworkCore;
using TrainersHub.Models;

namespace JwtAuthExample.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<RefreshToken>().HasKey(r => r.Id);

        modelBuilder.Entity<User>()
            .HasMany<RefreshToken>()
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId);
    }
}
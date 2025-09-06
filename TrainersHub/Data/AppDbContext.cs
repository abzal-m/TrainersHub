using Microsoft.EntityFrameworkCore;
using TrainersHub.Models;
using TrainersHub.Models.Auth;

namespace JwtAuthExample.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Training> Trainings { get; set; }
    public DbSet<TrainingSegment> TrainingSegments { get; set; }
    public DbSet<TrainingResult> TrainingResults { get; set; }
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
        
        // ===== Trainings =====
        modelBuilder.Entity<Training>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);

            entity.HasOne(t => t.Trainer)
                .WithMany()
                .HasForeignKey(t => t.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Athlete)
                .WithMany()
                .HasForeignKey(t => t.AthleteId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
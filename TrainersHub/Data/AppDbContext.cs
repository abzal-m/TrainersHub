using Microsoft.EntityFrameworkCore;
using TrainersHub.Models;
using TrainersHub.Models.Auth;
using TrainersHub.Models.Strava;

namespace JwtAuthExample.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<TrainerAthlete> TrainerAthletes { get; set; }
    public DbSet<Training> Trainings { get; set; }
    public DbSet<TrainingSegment> TrainingSegments { get; set; }
    public DbSet<TrainingResult> TrainingResults { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<StravaToken> StravaTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== Users =====
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
        
        modelBuilder.Entity<TrainerAthlete>()
            .HasKey(u => u.Id);
        
        modelBuilder.Entity<TrainerAthlete>()
            .HasOne(t => t.Trainer)
            .WithMany(u => u.Athletes) // у тренера много атлетов
            .HasForeignKey(t => t.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrainerAthlete>()
            .HasOne(t => t.Athlete)
            .WithOne(u => u.TrainerLink) // у атлета один тренер
            .HasForeignKey<TrainerAthlete>(t => t.AthleteId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== RefreshTokens =====
        modelBuilder.Entity<RefreshToken>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<User>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== Trainings =====
        modelBuilder.Entity<Training>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);

            entity.HasOne(t => t.Trainer)
                .WithMany(u => u.TrainingsAsTrainer)
                .HasForeignKey(t => t.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Athlete)
                .WithMany(u => u.TrainingsAsAthlete)
                .HasForeignKey(t => t.AthleteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== StravaTokens =====
        modelBuilder.Entity<StravaToken>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<StravaToken>()
            .HasOne(s => s.User)
            .WithMany(u => u.StravaTokens)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

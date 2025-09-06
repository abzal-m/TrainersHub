using Microsoft.EntityFrameworkCore;

namespace TrainersHub.Data;

public class TrainingDbContext : DbContext
{
    public TrainingDbContext(DbContextOptions<TrainingDbContext> options) : base(options) { }

    public DbSet<Data.Models.AppUser> Users { get; set; }
    public DbSet<Data.Models.Training> Trainings { get; set; }
    public DbSet<Data.Models.TrainingResult> TrainingResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Уникальный Email
        modelBuilder.Entity<Data.Models.AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Связь "Тренировка -> Создатель"
        modelBuilder.Entity<Data.Models.Training>()
            .HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Связь "Результаты -> Атлет"
        modelBuilder.Entity<Data.Models.TrainingResult>()
            .HasOne(r => r.Athlete)
            .WithMany(u => u.TrainingResults)
            .HasForeignKey(r => r.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь "Результаты -> Тренировка"
        modelBuilder.Entity<Data.Models.TrainingResult>()
            .HasOne(r => r.Training)
            .WithMany(t => t.Results)
            .HasForeignKey(r => r.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
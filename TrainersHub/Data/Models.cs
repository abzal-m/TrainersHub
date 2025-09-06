namespace TrainersHub.Data;

public class Models
{
    public enum UserRole
    {
        Admin,
        Coach,
        Athlete
    }

    public class AppUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = ""; // упрощённо, без хеширования
        public UserRole Role { get; set; }

        public ICollection<TrainingResult> TrainingResults { get; set; } = new List<TrainingResult>();
    }

    public class Training
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public DateTime ScheduledTime { get; set; }
        public string Description { get; set; } = "";

        public int CreatedById { get; set; }
        public AppUser CreatedBy { get; set; } = null!;
        public ICollection<TrainingResult> Results { get; set; } = new List<TrainingResult>();
    }

    public class TrainingResult
    {
        public int Id { get; set; }
        public int AthleteId { get; set; }
        public AppUser Athlete { get; set; } = null!;

        public int TrainingId { get; set; }
        public Training Training { get; set; } = null!;

        public int AverageHeartRate { get; set; }
        public TimeSpan Duration { get; set; }
        public double DistanceKm { get; set; }
        public double ElevationGain { get; set; }
    }
}
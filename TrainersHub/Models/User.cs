namespace TrainersHub.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; 
    public string Role { get; set; } = "Athlete";
}

public class Training
{
    public int Id { get; set; }
    public int TrainerId { get; set; }
    public User Trainer { get; set; }
    public int AthleteId { get; set; }
    public User Athlete { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TrainingSegment> Segments { get; set; } = new List<TrainingSegment>();
    public ICollection<TrainingResult> Results { get; set; } = new List<TrainingResult>();
}

public class TrainingSegment
{
    public int Id { get; set; }
    public int TrainingId { get; set; }
    public Training Training { get; set; }
    public int Order { get; set; }
    public int TargetHeartRate { get; set; }
    public int TargetCadence { get; set; }
    public int DurationMinutes { get; set; }
    public double DistanceKm { get; set; }
}

public class TrainingResult
{
    public int Id { get; set; }
    public int TrainingId { get; set; }
    public Training Training { get; set; }
    public int AthleteId { get; set; }
    public User Athlete { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int ElevationGain { get; set; }
    public int AvgHeartRate { get; set; }
    public int AvgCadence { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
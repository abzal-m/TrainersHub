namespace TrainersHub.Models;

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
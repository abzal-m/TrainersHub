namespace TrainersHub.Models;

public class Training
{
    public int Id { get; set; }
    public int TrainerId { get; set; }
    public User Trainer { get; set; }
    public int AthleteId { get; set; }
    public User Athlete { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public DateTime TrainingDay { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TrainingSegment> Segments { get; set; } = new List<TrainingSegment>();
    public ICollection<TrainingResult> Results { get; set; } = new List<TrainingResult>();
}
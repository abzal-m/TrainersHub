namespace TrainersHub.Models.TrainingModels;

public class TrainingSegmentDto
{
    public int Order { get; set; }
    public int TargetHeartRate { get; set; }
    public int TargetCadence { get; set; }
    public int DurationMinutes { get; set; }
    public double DistanceKm { get; set; }
}
public class TrainingViewDto
{
    public int TrainingId { get; set; }
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public DateTime TrainingDay { get; set; } = DateTime.Today;
    public string TrainerName { get; set; } = string.Empty;
    public List<TrainingSegmentDto> Segments { get; set; } = new();
}
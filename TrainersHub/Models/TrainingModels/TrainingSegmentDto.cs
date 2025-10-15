namespace TrainersHub.Models.TrainingModels;

public class TrainingSegmentDto
{
    public int Order { get; set; }
    public int TargetHeartRate { get; set; }
    public int TargetCadence { get; set; }
    public int DurationMinutes { get; set; }
    public double DistanceKm { get; set; }
}

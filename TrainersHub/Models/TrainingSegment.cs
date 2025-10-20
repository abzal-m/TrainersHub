namespace TrainersHub.Models;

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
namespace TrainersHub.Models.TrainingModels;

public class TrainingResultDto
{
    public int TrainingId { get; set; }
    public int AthleteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int ElevationGain { get; set; }
    public int AvgHeartRate { get; set; }
    public int AvgCadence { get; set; }
}
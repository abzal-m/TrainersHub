namespace TrainersHub.Models.TrainingModels;

public class TrainingResultDto
{
    public int TrainingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int ElevationGain { get; set; }
    public int AvgHeartRate { get; set; }
    public int AvgCadence { get; set; }
    public int Rpe { get; set; }
    public string Wellbeing { get; set; } = string.Empty;
    public string AthleteNotion { get; set; } = string.Empty;
    
    
}
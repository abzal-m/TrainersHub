namespace TrainersHub.Models.TrainingModels;

public class CreateTrainingDto
{
    public int TrainerId { get; set; }
    public int AthleteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TrainingDay { get; set; }
    public List<TrainingSegmentDto> Segments { get; set; } = new();
}
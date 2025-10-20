namespace TrainersHub.Models.TrainingModels;

public class ShortTrainingsForCalendarDto
{
    public int TrainingId { get; set; }
    public string Title { get; set; }
    public DateTime TrainingDay { get; set; } = DateTime.Today;
    public bool IsDone  { get; set; }
}
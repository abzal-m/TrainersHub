namespace TrainersHub.Models.TrainingModels;

public class TrainingResponseModel
{
    public TrainingViewDto TodayTraining { get; set; }
    public IEnumerable<TrainingViewDto> FutureTraining { get; set; }
}
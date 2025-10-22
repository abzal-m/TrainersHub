namespace TrainersHub.Models;

public class TrainerAthlete
{
    public int Id { get; set; }
    public int TrainerId { get; set; }
    public User Trainer { get; set; }   // 🔗 один тренер

    public int AthleteId { get; set; }
    public User Athlete { get; set; }
}
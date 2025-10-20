using TrainersHub.Models.Auth;
using TrainersHub.Models.Strava;

namespace TrainersHub.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; 
    public string Role { get; set; } = "Athlete";
    
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Training> TrainingsAsTrainer { get; set; } = new List<Training>();
    public ICollection<Training> TrainingsAsAthlete { get; set; } = new List<Training>();
    public ICollection<StravaToken> StravaTokens { get; set; } = new List<StravaToken>();
}





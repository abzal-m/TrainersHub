namespace TrainersHub.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // ⚠️ в реальном проекте хэшируй!
    public string Role { get; set; } = "Athlete";
}
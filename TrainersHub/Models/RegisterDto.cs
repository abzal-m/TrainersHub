namespace TrainersHub.Models;

public class RegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Новое поле для выбора роли
    public string Role { get; set; } = "Athlete"; // по умолчанию "Athlete"
}
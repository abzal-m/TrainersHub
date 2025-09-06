namespace TrainersHub.Models;

public class User
{
    public string Username { get; set; }
    public string Password { get; set; } // ⚠️ хранить хэши в реале!
    public string Role { get; set; }

    public User(string username, string password, string role)
    {
        Username = username;
        Password = password;
        Role = role;
    }
}
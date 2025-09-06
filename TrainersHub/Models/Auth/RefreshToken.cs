namespace TrainersHub.Models.Auth;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }
}
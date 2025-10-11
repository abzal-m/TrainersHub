using TrainersHub.Models.Strava;

namespace TrainersHub;

public interface IStravaService
{
    Task<string> RefreshAccessToken(string refreshToken);
    Task<StravaStatsModel> GetStats(string token, string id);
    Task<(string accessToken, string refreshToken)> ExchangeCodeForToken(string authCode);
    Task<List<ActivityModel>?> GetLastActivity(string token);
}
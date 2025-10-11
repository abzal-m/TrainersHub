using System.Text.Json;
using Newtonsoft.Json.Linq;
using TrainersHub.Models.Strava;

namespace TrainersHub.Services;

public class StravaService : IStravaService
{
    private static readonly string ClientId = "174332";
    private static readonly string ClientSecret = "245208f5c5e460e8db3d02495583b507293d6c6e";
    
    public async Task<string> RefreshAccessToken(string refreshToken)
    {
        using var client = new HttpClient();
        var data = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", ClientId),
            new KeyValuePair<string, string>("client_secret", ClientSecret),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
        });

        var response = await client.PostAsync("https://www.strava.com/oauth/token", data);
        var json = JObject.Parse(await response.Content.ReadAsStringAsync());

        return json["access_token"]?.ToString()!;
    }

    public async Task<StravaStatsModel> GetStats(string token, string id)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var response = await client.GetAsync($"https://www.strava.com/api/v3/athletes/{id}/stats");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var stravaStatsModel = StravaStatsModel.FromJson(json);
        return stravaStatsModel;
    }

    public async Task<(string accessToken, string refreshToken)> ExchangeCodeForToken(string authCode)
    {
        using var client = new HttpClient();
        var data = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", ClientId),
            new KeyValuePair<string, string>("client_secret", ClientSecret),
            new KeyValuePair<string, string>("code", authCode),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
        });

        var response = await client.PostAsync("https://www.strava.com/oauth/token", data);
        var json = JObject.Parse(await response.Content.ReadAsStringAsync());

        return (json["access_token"]?.ToString()!, json["refresh_token"]?.ToString()!);
    }

    public async Task<List<ActivityModel>?> GetLastActivity(string token)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var response = await client.GetAsync("https://www.strava.com/api/v3/athlete/activities?per_page=20&page=1");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ActivityModel>>(json);
    }
}
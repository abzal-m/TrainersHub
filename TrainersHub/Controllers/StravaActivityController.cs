using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TrainersHub.Models;

namespace TrainersHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StravaActivityController : Controller
{
    private readonly ILogger<StravaActivityController> _logger;
    private static readonly string clientId = "174332";
    private static readonly string clientSecret = "245208f5c5e460e8db3d02495583b507293d6c6e";
    private static readonly string redirectUri = "http://localhost/exchange_token";
    private static readonly string tokenFile = "strava_tokens.json";
//26d2792f52da1dcce49a5bfcd0b2790c0f7adf13
//https://www.strava.com/oauth/authorize?client_id=174332&response_type=code&redirect_uri=http://localhost/exchange_token&approval_prompt=force&scope=read,activity:read_all

    public StravaActivityController(ILogger<StravaActivityController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet("GetLastActivity")]
    public async Task<ActionResult<List<ActivityModel>>> Get()
    {
        try
        {
            var (accessToken, refreshToken) = LoadTokens();

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return Unauthorized("No saved Strava tokens. Please authenticate first.");

            // Пробуем обновить токен
            accessToken = await RefreshAccessToken(refreshToken);
            SaveTokens(accessToken, refreshToken);

            var activities = await GetLastActivity(accessToken);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching Strava activities");
            return StatusCode(500, "Internal Server Error");
        }
    }

    private async Task<(string accessToken, string refreshToken)> ExchangeCodeForToken(string authCode)
    {
        using var client = new HttpClient();
        var data = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("code", authCode),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
        });

        var response = await client.PostAsync("https://www.strava.com/oauth/token", data);
        var json = JObject.Parse(await response.Content.ReadAsStringAsync());

        return (json["access_token"]?.ToString()!, json["refresh_token"]?.ToString()!);
    }

    private async Task<string> RefreshAccessToken(string refreshToken)
    {
        using var client = new HttpClient();
        var data = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
        });

        var response = await client.PostAsync("https://www.strava.com/oauth/token", data);
        var json = JObject.Parse(await response.Content.ReadAsStringAsync());

        return json["access_token"]?.ToString()!;
    }

    private void SaveTokens(string accessToken, string refreshToken)
    {
        var json = new JObject
        {
            ["access_token"] = accessToken,
            ["refresh_token"] = refreshToken
        };
        System.IO.File.WriteAllText(tokenFile, json.ToString());
    }

    private (string accessToken, string refreshToken) LoadTokens()
    {
        if (!System.IO.File.Exists(tokenFile))
            return (string.Empty, string.Empty);

        var json = JObject.Parse(System.IO.File.ReadAllText(tokenFile));
        return (json["access_token"]?.ToString() ?? "", json["refresh_token"]?.ToString() ?? "");
    }

    private async Task<List<ActivityModel>?> GetLastActivity(string token)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var response = await client.GetAsync("https://www.strava.com/api/v3/athlete/activities?per_page=10&page=1");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ActivityModel>>(json);
    }
}

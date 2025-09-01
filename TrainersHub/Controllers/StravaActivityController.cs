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
    
    [HttpGet(Name = "GetLastActivity")]
    public async Task<List<ActivityModel>?> Get()
    {
        string accessToken;
        string refreshToken;

        if (!System.IO.File.Exists(tokenFile))
        {
            Console.WriteLine("Введите auth_code из URL после авторизации:");
            string authCode = Console.ReadLine();

            var tokens = await ExchangeCodeForToken(authCode);
            accessToken = tokens.accessToken;
            refreshToken = tokens.refreshToken;
            SaveTokens(accessToken, refreshToken);
        }
        else
        {
            var tokens = LoadTokens();
            accessToken = tokens.accessToken;
            refreshToken = tokens.refreshToken;

            // Пробуем обновить токен
            accessToken = await RefreshAccessToken(refreshToken);
            SaveTokens(accessToken, refreshToken);
        }

        // Получаем последнюю тренировку
        return await GetLastActivity(accessToken);
    }
    
    static async Task<(string accessToken, string refreshToken)> ExchangeCodeForToken(string authCode)
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

        Console.WriteLine("\n=== TOKEN RESPONSE ===");
        Console.WriteLine(json);

        return (json["access_token"]?.ToString(), json["refresh_token"]?.ToString());
    }

    static async Task<string> RefreshAccessToken(string refreshToken)
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

        Console.WriteLine("\n=== REFRESH TOKEN RESPONSE ===");
        Console.WriteLine(json);

        return json["access_token"]?.ToString();
    }

    static void SaveTokens(string accessToken, string refreshToken)
    {
        var json = new JObject
        {
            ["access_token"] = accessToken,
            ["refresh_token"] = refreshToken
        };
        System.IO.File.WriteAllText(tokenFile, json.ToString());
    }

    static (string accessToken, string refreshToken) LoadTokens()
    {
        var json = JObject.Parse(System.IO.File.ReadAllText(tokenFile));
        return (json["access_token"].ToString(), json["refresh_token"].ToString());
    }

    static async Task<List<ActivityModel>?> GetLastActivity(string token)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var response = await client.GetAsync("https://www.strava.com/api/v3/athlete/activities?per_page=10&page=1");
        var json = await response.Content.ReadAsStringAsync();
        var parsedData = JsonSerializer.Deserialize<List<ActivityModel>>(json);
        return parsedData;

        Console.WriteLine("\n=== LAST ACTIVITY ===");
        Console.WriteLine(JsonSerializer.Serialize(parsedData));
    }
}

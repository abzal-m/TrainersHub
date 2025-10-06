using System.Text.Json;
using JwtAuthExample.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using TrainersHub.Models;
using TrainersHub.Models.Strava;

namespace TrainersHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StravaActivityController : Controller
{
    private readonly ILogger<StravaActivityController> _logger;
    private readonly AppDbContext _context;
    private static readonly string clientId = "174332";
    private static readonly string clientSecret = "245208f5c5e460e8db3d02495583b507293d6c6e";
    private static readonly string redirectUri = "http://localhost/exchange_token";
//26d2792f52da1dcce49a5bfcd0b2790c0f7adf13
//https://www.strava.com/oauth/authorize?client_id=174332&response_type=code&redirect_uri=http://localhost/exchange_token&approval_prompt=force&scope=read,activity:read_all

    public StravaActivityController(ILogger<StravaActivityController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    /// <summary>
    /// Авторизация Strava: сохраняет токены в Postgres
    /// </summary>
    [HttpGet("Authorize")]
    public async Task<IActionResult> Authorize([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Auth code is required");

        try
        {
            // Берём ID текущего пользователя
            var userId = int.Parse(User.FindFirst("id")!.Value);
            
            var (accessToken, refreshToken) = await ExchangeCodeForToken(code);

            var tokenEntity = await _context.StravaTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tokenEntity == null)
            {
                tokenEntity = new StravaToken
                {
                    UserId = userId,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.StravaTokens.Add(tokenEntity);
            }
            else
            {
                tokenEntity.AccessToken = accessToken;
                tokenEntity.RefreshToken = refreshToken;
                tokenEntity.UpdatedAt = DateTime.UtcNow;
                _context.StravaTokens.Update(tokenEntity);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Strava tokens saved", tokenEntity.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Strava authorization");
            return StatusCode(500, "Internal Server Error");
        }
    }

    
    [HttpGet("GetLastActivity")]
    public async Task<ActionResult<List<ActivityModel>>> Get()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var tokens = await _context.StravaTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tokens == null)
                return Unauthorized("No Strava tokens for this user. Please authenticate first.");

            // обновляем токен
            var accessToken = await RefreshAccessToken(tokens.RefreshToken);
            tokens.AccessToken = accessToken;
            tokens.UpdatedAt = DateTime.UtcNow;
            await SaveTokens(tokens);

            var activities = await GetLastActivity(accessToken);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching Strava activities");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("IsConnected")]
    public async Task<IActionResult> IsConnected()
    {
        var userId = int.Parse(User.FindFirst("id")!.Value);

        var hasToken = await _context.StravaTokens.AnyAsync(x => x.UserId == userId);
        return Ok(new { isConnected = hasToken });
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

  
    private async Task SaveTokens(StravaToken tokens)
    {
        if (tokens.Id == 0)
            _context.StravaTokens.Add(tokens);
        else
            _context.StravaTokens.Update(tokens);

        await _context.SaveChangesAsync();
    }

    private async Task<StravaToken?> LoadTokens()
    {
        return await _context.StravaTokens.OrderByDescending(t => t.UpdatedAt).FirstOrDefaultAsync();
    }

    private async Task<List<ActivityModel>?> GetLastActivity(string token)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var response = await client.GetAsync("https://www.strava.com/api/v3/athlete/activities?per_page=20&page=1");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ActivityModel>>(json);
    }
}

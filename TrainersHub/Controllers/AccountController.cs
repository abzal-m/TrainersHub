using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TrainersHub.Models;

namespace JwtAuthExample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private static List<User> _users = new()
    {
        new User("admin", "12345", "Admin")
    };

    // Хранилище refresh токенов (пока в памяти)
    private static Dictionary<string, string> _refreshTokens = new();

    private readonly JwtOptions _jwtOptions;

    public AccountController(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    // ====== Регистрация ======
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (_users.Any(u => u.Username == request.Username))
            return BadRequest(new { message = "Пользователь уже существует" });

        _users.Add(new User(request.Username, request.Password, "Athlete"));
        return Ok(new { message = "Регистрация успешна" });
    }

    // ====== Логин ======
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _users.FirstOrDefault(u =>
            u.Username == request.Username && u.Password == request.Password);

        if (user is null)
            return Unauthorized();

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Сохраняем refreshToken -> username
        _refreshTokens[refreshToken] = user.Username;

        return Ok(new
        {
            accessToken,
            refreshToken
        });
    }

    // ====== Обновление токена ======
    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        if (!_refreshTokens.TryGetValue(request.RefreshToken, out var username))
            return Unauthorized(new { message = "Неверный refresh token" });

        var user = _users.FirstOrDefault(u => u.Username == username);
        if (user is null)
            return Unauthorized();

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // Заменяем refresh token
        _refreshTokens.Remove(request.RefreshToken);
        _refreshTokens[newRefreshToken] = username;

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken
        });
    }

    // ====== Защищённый эндпоинт ======
    [Authorize]
    [HttpGet("secure")]
    public IActionResult Secure()
    {
        return Ok(new
        {
            Message = "Добро пожаловать!",
            User = User.Identity?.Name,
            Role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }

    // ====== Методы ======
    private string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpireMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}

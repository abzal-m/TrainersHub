using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using TrainersHub.Models;

namespace JwtAuthExample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    // Мок-пользователи
    private static List<User> _users = new()
    {
        new User("admin", "12345", "Admin")
    };

    private readonly JwtOptions _jwtOptions;

    public AccountController(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    // ===== Регистрация =====
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (_users.Any(u => u.Username == request.Username))
            return BadRequest(new { message = "Пользователь уже существует" });

        var newUser = new User(request.Username, request.Password, "Athlete");
        _users.Add(newUser);

        return Ok(new { message = "Регистрация успешна" });
    }

    // ===== Логин =====
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _users.FirstOrDefault(u =>
            u.Username == request.Username && u.Password == request.Password);

        if (user is null)
            return Unauthorized();

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
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpireMinutes),
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { token = jwt });
    }

    // ===== Защищенный эндпоинт =====
    [Authorize]
    [HttpGet("secure")]
    public IActionResult Secure()
    {
        var username = User.Identity?.Name;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            Message = "Добро пожаловать!",
            User = username,
            Role = role
        });
    }
}

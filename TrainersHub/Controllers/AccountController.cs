using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JwtAuthExample.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TrainersHub.Models;
using TrainersHub.Models.Auth;

namespace TrainersHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtOptions _jwtOptions;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AccountController(AppDbContext db, IOptions<JwtOptions> jwtOptions, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtOptions = jwtOptions.Value;
    }

    // ====== Регистрация ======
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest(new { message = "Пользователь уже существует" });

        var allowedRoles = new[] { "Trainer", "Athlete", "Admin" };
        if (!allowedRoles.Contains(request.Role))
            return BadRequest("Invalid role. Allowed: Trainer, Athlete, Admin");
        
        var user = new User
        {
            Username = request.Username,
            Role = request.Role,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(null!, request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { user.Id, user.Username, user.Role });
    }

    // ====== Логин ======
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
            return Unauthorized();

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid username or password");

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Сохраняем refresh-токен в БД
        _db.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpireDays),
            UserId = user.Id
        });
        await _db.SaveChangesAsync();

        // Устанавливаем HttpOnly cookie
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // обязательно в проде (HTTPS)
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpireDays)
        });

        return Ok(new { accessToken });
    }

    // ====== Обновление токена ======
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Refresh token отсутствует" });

        var refresh = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (refresh == null || refresh.ExpiryDate < DateTime.UtcNow)
            return Unauthorized(new { message = "Refresh token недействителен" });

        var newAccessToken = GenerateAccessToken(refresh.User);
        var newRefreshToken = GenerateRefreshToken();

        // Обновляем refresh-токен в базе
        refresh.Token = newRefreshToken;
        refresh.ExpiryDate = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpireDays);
        await _db.SaveChangesAsync();

        // Перезаписываем cookie
        Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpireDays)
        });

        return Ok(new { accessToken = newAccessToken });
    }

    // ====== Выход ======
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Ok(new { message = "Вы уже вышли" });

        var refresh = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
        if (refresh != null)
        {
            _db.RefreshTokens.Remove(refresh);
            await _db.SaveChangesAsync();
        }

        // Удаляем cookie
        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Вы успешно вышли" });
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
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
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

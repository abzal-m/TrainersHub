using Microsoft.AspNetCore.Mvc;
using TrainersHub.Data;
using TrainersHub.Helper;

namespace TrainersHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TrainingDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(TrainingDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] Data.Models.AppUser request)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == request.Email && u.Password == request.Password);
        if (user == null) return Unauthorized();

        var token = JwtHelper.GenerateJwtToken(user, _config);
        return Ok(new { token, role = user.Role.ToString() });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] Data.Models.AppUser user)
    {
        if (_db.Users.Any(u => u.Email == user.Email)) return BadRequest("Email already exists");

        _db.Users.Add(user);
        _db.SaveChanges();
        return Ok(user);
    }
}
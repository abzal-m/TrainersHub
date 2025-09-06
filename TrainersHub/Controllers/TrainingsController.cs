using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainersHub.Data;

namespace TrainersHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainingsController : ControllerBase
{
    private readonly TrainingDbContext _db;

    public TrainingsController(TrainingDbContext db) => _db = db;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Trainings.Include(t => t.CreatedBy).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Create([FromBody] Data.Models.Training training)
    {
        _db.Trainings.Add(training);
        await _db.SaveChangesAsync();
        return Ok(training);
    }
}
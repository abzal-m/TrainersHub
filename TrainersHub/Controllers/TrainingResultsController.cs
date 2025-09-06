using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainersHub.Data;

namespace TrainersHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainingResultsController : ControllerBase
{
    private readonly TrainingDbContext _db;

    public TrainingResultsController(TrainingDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.TrainingResults.Include(r => r.Athlete).Include(r => r.Training).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> AddResult([FromBody] Data.Models.TrainingResult result)
    {
        _db.TrainingResults.Add(result);
        await _db.SaveChangesAsync();
        return Ok(result);
    }
}
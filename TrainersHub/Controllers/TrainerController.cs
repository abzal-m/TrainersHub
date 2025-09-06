using JwtAuthExample.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainersHub.Models;
using TrainersHub.Models.TrainingModels;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Trainer")]
public class TrainerController : ControllerBase
{
    private readonly AppDbContext _context;
    public TrainerController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("CreateTraining")]
    public async Task<IActionResult> CreateTraining([FromBody] CreateTrainingDto dto)
    {
        var athlete = await _context.Users.FindAsync(dto.AthleteId);
        if (athlete == null || athlete.Role != "Athlete")
        {
            return BadRequest("Athlete not found or invalid role");
        }
        
        var training = new Training
        {
            TrainerId = dto.TrainerId,
            AthleteId = dto.AthleteId,
            Title = dto.Title,
            CreatedAt = DateTime.UtcNow,
            Segments = dto.Segments.Select(s => new TrainingSegment
            {
                Order = s.Order,
                TargetHeartRate = s.TargetHeartRate,
                TargetCadence = s.TargetCadence,
                DurationMinutes = s.DurationMinutes,
                DistanceKm = s.DistanceKm
            }).ToList()
        };

        _context.Trainings.Add(training);
        await _context.SaveChangesAsync();

        return Ok(new { training.Id, training.Title });
    }

    [HttpGet("AthleteTrainings/{athleteId}")]
    public async Task<IActionResult> GetTrainingsForAthlete(int athleteId)
    {
        var trainings = await _context.Trainings
            .Include(t => t.Segments)
            .Where(t => t.AthleteId == athleteId)
            .ToListAsync();

        return Ok(trainings);
    }
}
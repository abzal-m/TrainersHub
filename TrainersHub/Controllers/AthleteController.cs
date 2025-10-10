using System.Security.Claims;
using JwtAuthExample.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainersHub.Models;
using TrainersHub.Models.TrainingModels;

[ApiController]
[Authorize(Roles = "Athlete")]
[Route("api/[controller]")]
public class AthleteController : ControllerBase
{
    private readonly AppDbContext _context;
    public AthleteController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("UploadResult")]
    public async Task<IActionResult> UploadResult([FromBody] TrainingResultDto dto)
    {
        
        var athleteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (athleteIdClaim == null) return Unauthorized();

        int athleteId = int.Parse(athleteIdClaim);
        
        var training = await _context.Trainings.FindAsync(dto.TrainingId);
        if (training == null) return NotFound("Training not found");

        var result = new TrainingResult
        {
            TrainingId = dto.TrainingId,
            AthleteId = athleteId,
            Title = dto.Title,
            DurationMinutes = dto.DurationMinutes,
            ElevationGain = dto.ElevationGain,
            AvgHeartRate = dto.AvgHeartRate,
            AvgCadence = dto.AvgCadence,
            CreatedAt = DateTime.UtcNow
        };

        _context.TrainingResults.Add(result);
        await _context.SaveChangesAsync();

        return Ok(new { result.Id, result.Title });
    }

    [HttpGet("MyResults")]
    public async Task<IActionResult> GetMyResults()
    {
        var athleteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (athleteIdClaim == null) return Unauthorized();

        int athleteId = int.Parse(athleteIdClaim);
        
        var results = await _context.TrainingResults
            .Where(r => r.AthleteId == athleteId)
            .ToListAsync();

        return Ok(results);
    }
    [HttpGet("Trainings")]
    public async Task<IActionResult> GetMyTrainings()
    {
        // достаём id атлета из токена
        var athleteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (athleteIdClaim == null) return Unauthorized();

        int athleteId = int.Parse(athleteIdClaim);

        var trainings = await _context.Trainings
            .Include(t => t.Trainer)
            .Include(t => t.Segments)
            .Where(t => t.AthleteId == athleteId)
            .Select(t => new TrainingViewDto
            {
                TrainingId = t.Id,
                Title = t.Title,
                TrainerName = t.Trainer.Username,
                TrainingDay = t.TrainingDay,
                Description = t.Description,
                Segments = t.Segments.Select(s => new TrainingSegmentDto
                {
                    Order = s.Id,
                    TargetHeartRate = s.TargetHeartRate,
                    TargetCadence = s.TargetCadence,
                    DistanceKm = s.DistanceKm,
                    DurationMinutes = s.DurationMinutes
                }).ToList()
            })
            .ToListAsync();

        return Ok(trainings);
    }
    
}
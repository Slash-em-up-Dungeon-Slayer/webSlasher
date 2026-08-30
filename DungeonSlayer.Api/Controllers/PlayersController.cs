using System.Security.Claims;
using DungeonSlayer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DungeonSlayer.Api.Controllers;

[ApiController]
[Route("api/players")]
[Authorize]
public class PlayersController : ControllerBase
{
    private readonly ProgressService _progressService;

    public PlayersController(ProgressService progressService)
    {
        _progressService = progressService;
    }

    [HttpGet("me/progress")]
    public async Task<IActionResult> GetMyProgress()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var progress = await _progressService.GetOrCreateProgressAsync(userId);
        return Ok(new
        {
            level = progress.Level,
            experience = progress.Experience,
            skillPoints = progress.SkillPoints,
            maxHealthPoints = progress.MaxHealthPoints,
            highestLevelCompleted = progress.HighestLevelCompleted,
            totalKills = progress.TotalKills
        });
    }

    [HttpPost("runs")]
    public async Task<IActionResult> SubmitRunResult([FromBody] RunResultDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var updated = await _progressService.ProcessRunResultAsync(userId, dto);
        return Ok(new
        {
            level = updated.Level,
            experience = updated.Experience,
            skillPoints = updated.SkillPoints,
            maxHealthPoints = updated.MaxHealthPoints,
            highestLevelCompleted = updated.HighestLevelCompleted,
            totalKills = updated.TotalKills
        });
    }
}

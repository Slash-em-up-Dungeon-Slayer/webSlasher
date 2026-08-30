using DungeonSlayer.Api.Data;
using DungeonSlayer.Shared.Configs;
using DungeonSlayer.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace DungeonSlayer.Api.Services;

public class ProgressService
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<int, LevelConfig> _levelConfigs;

    public ProgressService(ApplicationDbContext context)
    {
        _context = context;
        _levelConfigs = LevelConfigs.GetAll();
    }

    public async Task<PlayerProgress> GetOrCreateProgressAsync(int playerId)
    {
        var progress = await _context.PlayerProgresses.FindAsync(playerId);
        if (progress == null)
        {
            progress = new PlayerProgress { PlayerId = playerId };
            _context.PlayerProgresses.Add(progress);
            await _context.SaveChangesAsync();
        }
        return progress;
    }

    public async Task<PlayerProgress> ProcessRunResultAsync(int playerId, RunResultDto dto)
    {
        var progress = await GetOrCreateProgressAsync(playerId);
        var config = _levelConfigs.GetValueOrDefault(dto.LevelNumber);
        if (config == null) throw new ArgumentException("Invalid level");

        int maxXp = config.TotalEnemiesToSpawn * config.MaxXpPerEnemy;
        int validXp = Math.Min(dto.XpGained, maxXp);

        progress.Experience += validXp;
        progress.TotalKills += dto.KillCount;
        if (dto.LevelNumber > progress.HighestLevelCompleted)
            progress.HighestLevelCompleted = dto.LevelNumber;

        while (progress.Experience >= XpTable.RequiredXpForLevel(progress.Level))
        {
            progress.Experience -= XpTable.RequiredXpForLevel(progress.Level);
            progress.Level++;
            progress.SkillPoints++;
        }
        progress.UpdatedAtUtc = DateTime.UtcNow;

        var run = new RunResult
        {
            PlayerId = playerId,
            LevelNumber = dto.LevelNumber,
            KillCount = dto.KillCount,
            XpGained = validXp,
            DurationSeconds = dto.DurationSeconds,
            ClientTimestampUtc = dto.ClientTimestampUtc
        };
        _context.RunResults.Add(run);

        await _context.SaveChangesAsync();
        return progress;
    }

    public static class XpTable
    {
        public static int RequiredXpForLevel(int level) => (int)(100 * Math.Pow(level, 1.5));
    }
}

public class RunResultDto
{
    public int LevelNumber { get; set; }
    public int KillCount { get; set; }
    public int XpGained { get; set; }
    public float DurationSeconds { get; set; }
    public DateTime ClientTimestampUtc { get; set; }
}
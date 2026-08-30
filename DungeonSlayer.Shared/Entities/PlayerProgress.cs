namespace DungeonSlayer.Shared.Entities;

public class PlayerProgress
{
    public int PlayerId { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; } = 0;
    public int SkillPoints { get; set; } = 0;
    public int MaxHealthPoints { get; set; } = 100;
    public int HighestLevelCompleted { get; set; } = 0;
    public int TotalKills { get; set; } = 0;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

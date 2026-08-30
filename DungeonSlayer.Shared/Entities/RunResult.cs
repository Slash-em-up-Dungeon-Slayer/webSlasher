namespace DungeonSlayer.Shared.Entities;

public class RunResult
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int LevelNumber { get; set; }
    public int KillCount { get; set; }
    public int XpGained { get; set; }
    public float DurationSeconds { get; set; }
    public DateTime ClientTimestampUts { get; set; }
    public DateTime ServerRecordedAtUtc { get; set; } = DateTime.UtcNow;
}
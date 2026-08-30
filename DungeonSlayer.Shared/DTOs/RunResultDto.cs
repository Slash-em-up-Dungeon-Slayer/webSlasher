namespace DungeonSlayer.Shared.Dtos;

public class RunResultDto
{
    public int LevelNumber { get; set; }
    public int KillCount { get; set; }
    public int XpGained { get; set; }
    public float DurationSeconds { get; set; }
    public DateTime ClientTimestampUtc { get; set; }
}
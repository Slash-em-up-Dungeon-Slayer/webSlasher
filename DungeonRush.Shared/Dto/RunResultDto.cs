namespace DungeonRush.Shared.Dto;

public class RunResultDto
{
    public int EnemiesKilled { get; set; }
    public float DurationSeconds { get; set; }
    public int Score { get; set; }
    public DateTime ClientTimestampUtc { get; set; }
}

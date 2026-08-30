namespace DungeonSlayer.Shared.Configs;

public class LevelConfig
{
    public int LevelNumber { get; set; }
    public int MaxAliveEnemies { get; set; } = 10;
    public int TotalEnemiesSpawn { get; set; } = 10;
    public Dictionary<EnemyColor, float> SpawnWeights { get; set; } = new { };
    public int MaxXpPerEnemy { get; set; } = 10; //для валидации
}
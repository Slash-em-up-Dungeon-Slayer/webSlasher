using DungeonSlayer.Shared.Configs;
using DungeonSlayer.Shared.Enums;

namespace DungeonSlayer.Api.Configs;

public static class LevelConfigs
{
    public static Dictionary<int, LevelConfig> GetAll()
    {
        return new Dictionary<int, LevelConfig>
        {
            [1] = new LevelConfig
            {
                LevelNumber = 1,
                MaxAliveEnemies = 5,
                TotalEnemiesToSpawn = 10,
                MaxXpPerEnemy = 10,
                SpawnWeights = new Dictionary<EnemyColor, float>
                {
                    [EnemyColor.Red] = 0.4f,
                    [EnemyColor.Blue] = 0.3f,
                    [EnemyColor.White] = 0.2f,
                    [EnemyColor.Black] = 0.1f,
                }
            },
        };
    }
}
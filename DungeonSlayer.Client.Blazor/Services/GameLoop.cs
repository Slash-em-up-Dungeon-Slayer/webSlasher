using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using DungeonRush.Shared.Configs;
using DungeonRush.Shared.Enums;
using Microsoft.JSInterop;

namespace DungeonRush.Client.Blazor.Services;

public class GameLoop : IDisposable
{
    private readonly Canvas2DContext _ctx;
    private readonly ApiClient _api;
    private readonly Action<int, int> _onLevelComplete;
    private readonly IJSRuntime _js;

    private bool _running;
    private double _lastTime;
    private int _width, _height;

    // Игровые объекты
    private Player _player = null!;
    private List<Enemy> _enemies = new();
    private List<Projectile> _projectiles = new();
    private int _enemiesSpawned = 0;
    private int _enemiesKilled = 0;
    private int _totalEnemiesToSpawn = 10;
    private int _maxAliveEnemies = 5;
    private float _levelDuration = 0;

    // Ввод
    private Vector2 _moveDir = Vector2.Zero;
    private Vector2 _aimDir = Vector2.Zero;
    private bool _shootPressed = false;
    private float _shootCooldown = 0;
    private const float SHOOT_INTERVAL = 0.15f; // секунд

    // Конфиг уровня (захардкодим для MVP)
    private LevelConfig _config = new LevelConfig
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
    };

    // Параметры врагов (цвет -> характеристики)
    private Dictionary<EnemyColor, (float speed, int damage, int hp, int xp)> _enemyStats =
        new Dictionary<EnemyColor, (float, int, int, int)>
        {
            [EnemyColor.Red] = (1.0f, 10, 20, 10),
            [EnemyColor.Blue] = (0.8f, 15, 15, 12),
            [EnemyColor.White] = (0.6f, 5, 30, 8),
            [EnemyColor.Black] = (1.2f, 20, 10, 15),
            [EnemyColor.Yellow] = (0.9f, 12, 18, 11),
            [EnemyColor.Orange] = (1.1f, 18, 12, 14),
            [EnemyColor.Pink] = (0.7f, 8, 25, 9),
            [EnemyColor.Purple] = (1.3f, 25, 8, 18),
        };

    // Размеры объектов
    private const float PLAYER_SIZE = 30f;
    private const float ENEMY_SIZE = 25f;
    private const float PROJECTILE_SIZE = 8f;
    private const float PROJECTILE_SPEED = 400f;
    private const float PLAYER_SPEED = 200f;

    // Камера
    private Vector2 _cameraOffset;

    // Таймер неуязвимости игрока
    private float _invulnerabilityTimer = 0;

    public GameLoop(Canvas2DContext ctx, ApiClient api, Action<int, int> onLevelComplete, IJSRuntime js)
    {
        _ctx = ctx;
        _api = api;
        _onLevelComplete = onLevelComplete;
        _js = js;
    }

    public void Start()
    {
        _running = true;
        _player = new Player { Position = new Vector2(0, 0), HP = 100, MaxHP = 100 };
        _lastTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
        _enemies.Clear();
        _projectiles.Clear();
        _enemiesSpawned = 0;
        _enemiesKilled = 0;
        _levelDuration = 0;
        _shootCooldown = 0;

        // Подписываемся на события ввода через JS
        AttachInputHandlers();

        // Запускаем цикл
        Loop();
    }

    private async void Loop()
    {
        while (_running)
        {
            var now = DateTime.Now.TimeOfDay.TotalMilliseconds;
            float deltaTime = (float)(now - _lastTime) / 1000f;
            _lastTime = now;

            if (deltaTime > 0.1f) deltaTime = 0.1f; // защита от зависаний

            Update(deltaTime);
            await Render();

            await Task.Delay(1);
        }
    }

    private void Update(float dt)
    {
        _levelDuration += dt;

        // Обновление игрока
        _player.Position += _moveDir * PLAYER_SPEED * dt;
        _player.Position = Clamp(_player.Position, -1000, 1000); // границы уровня

        // Неуязвимость
        if (_invulnerabilityTimer > 0) _invulnerabilityTimer -= dt;

        // Стрельба
        _shootCooldown -= dt;
        if (_shootPressed && _shootCooldown <= 0 && _aimDir.LengthSquared() > 0.01f)
        {
            FireProjectile();
            _shootCooldown = SHOOT_INTERVAL;
        }

        // Обновление снарядов
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            var p = _projectiles[i];
            p.Position += p.Direction * PROJECTILE_SPEED * dt;
            if (p.Position.X < -1100 || p.Position.X > 1100 || p.Position.Y < -1100 || p.Position.Y > 1100)
            {
                _projectiles.RemoveAt(i);
                continue;
            }

            // Проверка попадания во врагов
            bool hit = false;
            for (int j = _enemies.Count - 1; j >= 0; j--)
            {
                var e = _enemies[j];
                if (Vector2.Distance(p.Position, e.Position) < (PROJECTILE_SIZE + ENEMY_SIZE) / 2)
                {
                    e.HP -= p.Damage;
                    if (e.HP <= 0)
                    {
                        _enemies.RemoveAt(j);
                        _enemiesKilled++;
                        // начисление опыта игроку (временно храним)
                        _player.RunExperience += e.XpReward;
                    }
                    hit = true;
                    break;
                }
            }
            if (hit)
                _projectiles.RemoveAt(i);
        }

        // Обновление врагов (движение к игроку)
        foreach (var e in _enemies)
        {
            var dir = (_player.Position - e.Position).Normalized();
            e.Position += dir * e.Speed * dt;
        }

        // Проверка столкновения врагов с игроком
        foreach (var e in _enemies)
        {
            if (Vector2.Distance(_player.Position, e.Position) < (PLAYER_SIZE + ENEMY_SIZE) / 2)
            {
                if (_invulnerabilityTimer <= 0)
                {
                    _player.HP -= e.Damage;
                    _invulnerabilityTimer = 0.5f;
                    if (_player.HP <= 0)
                    {
                        // Игрок умер — завершаем уровень с неудачей? Для MVP просто перезапускаем
                        // Но мы сделаем просто выход в профиль
                        _running = false;
                        _onLevelComplete?.Invoke(_enemiesKilled, _player.RunExperience);
                        return;
                    }
                }
                break;
            }
        }

        // Спавн врагов
        if (_enemiesSpawned < _config.TotalEnemiesToSpawn && _enemies.Count < _config.MaxAliveEnemies)
        {
            SpawnEnemy();
            _enemiesSpawned++;
        }

        // Проверка завершения уровня (все враги заспавнены и убиты)
        if (_enemiesSpawned >= _config.TotalEnemiesToSpawn && _enemies.Count == 0 && _projectiles.Count == 0)
        {
            // Уровень завершён
            _running = false;
            // Отправляем результат на сервер
            try
            {
                var result = new RunResultDto
                {
                    LevelNumber = _config.LevelNumber,
                    KillCount = _enemiesKilled,
                    XpGained = _player.RunExperience,
                    DurationSeconds = _levelDuration,
                    ClientTimestampUtc = DateTime.UtcNow
                };
                await _api.PostAsync<object>("api/players/runs", result);
            }
            catch { /* игнорируем ошибки сети */ }
            _onLevelComplete?.Invoke(_enemiesKilled, _player.RunExperience);
        }
    }

    private void FireProjectile()
    {
        var dir = _aimDir.Normalized();
        _projectiles.Add(new Projectile
        {
            Position = _player.Position + dir * (PLAYER_SIZE / 2 + 5),
            Direction = dir,
            Damage = 15 // базовый урон
        });
    }

    private void SpawnEnemy()
    {
        // Выбираем цвет по весам
        var rand = Random.Shared;
        float totalWeight = _config.SpawnWeights.Values.Sum();
        float r = (float)rand.NextDouble() * totalWeight;
        EnemyColor chosenColor = _config.SpawnWeights.Last().Key;
        foreach (var kv in _config.SpawnWeights)
        {
            r -= kv.Value;
            if (r <= 0)
            {
                chosenColor = kv.Key;
                break;
            }
        }

        var stats = _enemyStats[chosenColor];
        // Позиция за пределами видимости, но в пределах уровня
        Vector2 spawnPos;
        float angle = (float)rand.NextDouble() * 2 * MathF.PI;
        float distance = 600 + rand.Next(100, 300);
        spawnPos = _player.Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
        spawnPos = Clamp(spawnPos, -1000, 1000);

        _enemies.Add(new Enemy
        {
            Position = spawnPos,
            Color = chosenColor,
            Speed = stats.speed,
            Damage = stats.damage,
            HP = stats.hp,
            MaxHP = stats.hp,
            XpReward = stats.xp
        });
    }

    private Vector2 Clamp(Vector2 v, float min, float max)
    {
        return new Vector2(Math.Clamp(v.X, min, max), Math.Clamp(v.Y, min, max));
    }

    private async Task Render()
    {
        await _ctx.ClearRectAsync(0, 0, _width, _height);

        // Рассчитываем камеру (центр на игроке)
        _cameraOffset = _player.Position - new Vector2(_width / 2, _height / 2);

        // Рисуем врагов
        foreach (var e in _enemies)
        {
            var screenPos = e.Position - _cameraOffset;
            var color = GetColor(e.Color);
            await _ctx.SetFillStyleAsync(color);
            await _ctx.FillRectAsync(screenPos.X - ENEMY_SIZE / 2, screenPos.Y - ENEMY_SIZE / 2, ENEMY_SIZE, ENEMY_SIZE);
        }

        // Рисуем снаряды
        await _ctx.SetFillStyleAsync("yellow");
        foreach (var p in _projectiles)
        {
            var screenPos = p.Position - _cameraOffset;
            await _ctx.FillRectAsync(screenPos.X - PROJECTILE_SIZE / 2, screenPos.Y - PROJECTILE_SIZE / 2, PROJECTILE_SIZE, PROJECTILE_SIZE);
        }

        // Рисуем игрока
        var playerScreen = _player.Position - _cameraOffset;
        await _ctx.SetFillStyleAsync("green");
        await _ctx.FillRectAsync(playerScreen.X - PLAYER_SIZE / 2, playerScreen.Y - PLAYER_SIZE / 2, PLAYER_SIZE, PLAYER_SIZE);

        // Индикатор HP
        await _ctx.SetFillStyleAsync("red");
        await _ctx.FillRectAsync(10, 10, 200, 20);
        await _ctx.SetFillStyleAsync("lime");
        float hpPercent = (float)_player.HP / _player.MaxHP;
        await _ctx.FillRectAsync(10, 10, 200 * hpPercent, 20);

        // Счётчик убийств
        await _ctx.SetFillStyleAsync("white");
        await _ctx.SetFontAsync("20px Arial");
        await _ctx.FillTextAsync($"Kills: {_enemiesKilled}", 10, 50);

        // Опыт текущего забега
        await _ctx.FillTextAsync($"XP: {_player.RunExperience}", 10, 80);
    }

    private string GetColor(EnemyColor color) => color switch
    {
        EnemyColor.Red => "#ff0000",
        EnemyColor.Blue => "#0000ff",
        EnemyColor.White => "#ffffff",
        EnemyColor.Black => "#000000",
        EnemyColor.Yellow => "#ffff00",
        EnemyColor.Orange => "#ffa500",
        EnemyColor.Pink => "#ff69b4",
        EnemyColor.Purple => "#800080",
        _ => "#888888"
    };

    // Вспомогательные методы для ввода (через JS)
    private async void AttachInputHandlers()
    {
        // Для десктопа: WASD и мышь
        await _js.InvokeVoidAsync("attachKeyboardHandler", DotNetObjectReference.Create(this));
        await _js.InvokeVoidAsync("attachMouseHandler", DotNetObjectReference.Create(this));
        // Для мобильных: touch на зонах (упрощённо, используем отдельные обработчики)
        await _js.InvokeVoidAsync("attachTouchHandlers", DotNetObjectReference.Create(this));
    }

    [JSInvokable]
    public void SetMoveDirection(float x, float y) => _moveDir = new Vector2(x, y);

    [JSInvokable]
    public void SetAimDirection(float x, float y) => _aimDir = new Vector2(x, y);

    [JSInvokable]
    public void SetShoot(bool pressed) => _shootPressed = pressed;

    [JSInvokable]
    public void SetCanvasSize(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public async Task StopAsync()
    {
        _running = false;
        await _js.InvokeVoidAsync("removeInputHandlers");
    }

    public void Dispose()
    {
        _running = false;
    }

    // Внутренние классы
    private class Player
    {
        public Vector2 Position { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int RunExperience { get; set; } = 0;
    }

    private class Enemy
    {
        public Vector2 Position { get; set; }
        public EnemyColor Color { get; set; }
        public float Speed { get; set; }
        public int Damage { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int XpReward { get; set; }
    }

    private class Projectile
    {
        public Vector2 Position { get; set; }
        public Vector2 Direction { get; set; }
        public int Damage { get; set; }
    }

    private struct Vector2
    {
        public float X, Y;
        public Vector2(float x, float y) { X = x; Y = y; }
        public static Vector2 Zero => new Vector2(0, 0);
        public float LengthSquared() => X * X + Y * Y;
        public Vector2 Normalized()
        {
            float len = MathF.Sqrt(LengthSquared());
            return len > 0 ? new Vector2(X / len, Y / len) : Zero;
        }
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 v, float f) => new Vector2(v.X * f, v.Y * f);
        public static float Distance(Vector2 a, Vector2 b) => MathF.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
    }
}
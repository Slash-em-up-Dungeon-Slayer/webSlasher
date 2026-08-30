namespace DungeonRush.Api.Security;

/// <summary>
/// Тонкая обёртка над BCrypt. Пароли пользователей никогда не хранятся
/// и не логируются в открытом виде — только хэш.
/// </summary>
public static class PasswordHasher
{
    // WorkFactor 12 — разумный баланс между безопасностью и скоростью на 2026 год.
    private const int WorkFactor = 12;

    public static string Hash(string plainPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);

    public static bool Verify(string plainPassword, string hash) =>
        BCrypt.Net.BCrypt.Verify(plainPassword, hash);
}

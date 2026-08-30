namespace DungeonRush.Api.Domain;

public class Player
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    // Счётчик неудачных попыток входа — базовая защита от подбора пароля.
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
}

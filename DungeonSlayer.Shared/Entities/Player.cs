namespace DungeonSlayer.Shared.Entities;

public class Player
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreateAtUtc { get; set; } = DateTime.UtcNow;
    public int FailedLoginAttemts { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
}
namespace DungeonRush.Api.Services;

public record AuthResult(bool Success, string? Token, string? Error);

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string password);
    Task<AuthResult> LoginAsync(string email, string password);
}

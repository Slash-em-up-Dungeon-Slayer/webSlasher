namespace DungeonRush.Client.Blazor.Services;

public class AuthState
{
    public string? JwtToken { get; private set; }
    public string? Email { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(JwtToken);

    public void SetToken(string token, string email)
    {
        JwtToken = token;
        Email = email;
    }

    public void Logout()
    {
        JwtToken = null;
        Email = null;
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DungeonRush.Api.Data;
using DungeonRush.Api.Domain;
using DungeonRush.Api.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DungeonRush.Api.Services;

public class AuthService : IAuthService
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResult> RegisterAsync(string email, string password)
    {
        email = email.Trim().ToLowerInvariant();

        var exists = await _db.Players.AnyAsync(p => p.Email == email);
        if (exists)
            return new AuthResult(false, null, "Пользователь с таким email уже зарегистрирован");

        var player = new Player
        {
            Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Players.Add(player);
        await _db.SaveChangesAsync();

        return new AuthResult(true, null, null);
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        email = email.Trim().ToLowerInvariant();
        var player = await _db.Players.FirstOrDefaultAsync(p => p.Email == email);

        // Намеренно одинаковое сообщение об ошибке для "нет пользователя" и
        // "неверный пароль" — чтобы не давать возможность перебором узнавать
        // зарегистрированные email-адреса.
        const string genericError = "Неверный email или пароль";

        if (player is null)
            return new AuthResult(false, null, genericError);

        if (player.LockedUntilUtc is { } lockedUntil && lockedUntil > DateTime.UtcNow)
            return new AuthResult(false, null, "Аккаунт временно заблокирован из-за многочисленных неудачных попыток входа");

        if (!PasswordHasher.Verify(password, player.PasswordHash))
        {
            player.FailedLoginAttempts++;
            if (player.FailedLoginAttempts >= MaxFailedAttempts)
            {
                player.LockedUntilUtc = DateTime.UtcNow.Add(LockoutDuration);
                player.FailedLoginAttempts = 0;
            }
            await _db.SaveChangesAsync();
            return new AuthResult(false, null, genericError);
        }

        player.FailedLoginAttempts = 0;
        player.LockedUntilUtc = null;
        await _db.SaveChangesAsync();

        var token = GenerateToken(player);
        return new AuthResult(true, token, null);
    }

    private string GenerateToken(Player player)
    {
        var key = _config["Jwt:Key"]!;
        var minutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var m) ? m : 15;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, player.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, player.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

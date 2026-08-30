using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DungeonSlayer.Api.Data;
using DungeonSlayer.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DungeonSlayer.Api.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<Player?> RegisterAsync(string email, string password)
    {
        if (await _context.Players.AnyAsync(p => p.Email == email))
            return null;

        var player = new Player
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        // Создаём прогресс
        _context.PlayerProgresses.Add(new PlayerProgress { PlayerId = player.Id });
        await _context.SaveChangesAsync();

        return player;
    }

    public async Task<(Player? player, string? token)> LoginAsync(string email, string password)
    {
        var player = await _context.Players.FirstOrDefaultAsync(p => p.Email == email);
        if (player == null || !BCrypt.Net.BCrypt.Verify(password, player.PasswordHash))
            return (null, null);

        var token = GenerateJwtToken(player);
        return (player, token);
    }

    private string GenerateJwtToken(Player player)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
                new Claim(ClaimTypes.Email, player.Email)
            }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

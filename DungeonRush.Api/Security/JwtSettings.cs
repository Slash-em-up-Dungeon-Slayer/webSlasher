using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DungeonRush.Api.Security;

/// <summary>
/// Все значения JWT берутся из конфигурации (в конечном счёте — из переменных
/// окружения / .env / Kubernetes Secret), никогда не хардкодятся в коде.
/// </summary>
public static class JwtSettings
{
    public static TokenValidationParameters GetValidationParameters(IConfiguration config)
    {
        var key = config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key не задан в конфигурации");

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = config["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = config["Jwt:Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30) // минимальный допуск, не 5 минут по умолчанию
        };
    }
}

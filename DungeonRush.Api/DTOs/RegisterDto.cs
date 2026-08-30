using System.ComponentModel.DataAnnotations;

namespace DungeonRush.Api.DTOs;

public class RegisterDto
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    // Минимальная длина - базовая проверка, полноценную политику паролей
    // (спецсимволы, энтропия) можно ужесточить позже без изменения контракта.
    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

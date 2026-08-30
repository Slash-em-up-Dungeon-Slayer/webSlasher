using DungeonRush.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonRush.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    public class RegisterRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class LoginRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var player = await _authService.RegisterAsync(request.Email, request.Password);
        if (player == null)
            return BadRequest(new { message = "Email already exists" });

        return Ok(new { message = "Registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (player, token) = await _authService.LoginAsync(request.Email, request.Password);
        if (player == null || token == null)
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(new { token, email = player.Email });
    }
}
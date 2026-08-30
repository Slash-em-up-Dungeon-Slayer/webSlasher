using DungeonRush.Api.DTOs;
using DungeonRush.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DungeonRush.Api.Controllers;

[ApiController]
[Route("auth")]
[EnableRateLimiting("AuthPolicy")] // защита от брутфорса на уровне всего контроллера
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var result = await _authService.RegisterAsync(dto.Email, dto.Password);
        if (!result.Success) return BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var result = await _authService.LoginAsync(dto.Email, dto.Password);
        if (!result.Success) return Unauthorized(new { error = result.Error });

        return Ok(new { token = result.Token });
    }
}

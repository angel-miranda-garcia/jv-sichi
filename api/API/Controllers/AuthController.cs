using Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (result is null)
            return Unauthorized(new { message = "Credenciales inválidas" });

        return Ok(new { token = result.Token, expiresAt = result.ExpiresAt });
    }

    public record LoginRequest(string Email, string Password);
}

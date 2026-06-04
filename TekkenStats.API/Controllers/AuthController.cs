using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TekkenStats.Application.DTOs.Auth;
using TekkenStats.Application.Services;

namespace TekkenStats.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto, CancellationToken ct = default)
    {
        var result = await authService.RegisterAsync(dto, ct);
        return result is null
            ? Conflict("Username or email already in use")
            : Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken ct = default)
    {
        var result = await authService.LoginAsync(dto, ct);
        return result is null
            ? Unauthorized("Invalid credentials")
            : Ok(result);
    }

    /// <summary>Link the authenticated user's account to their Tekken Polaris ID.</summary>
    [Authorize]
    [HttpPost("link-polaris")]
    public async Task<IActionResult> LinkPolaris(LinkPolarisIdDto dto, CancellationToken ct = default)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub") ?? "0");

        var ok = await authService.LinkPolarisIdAsync(userId, dto.PolarisId, ct);
        return ok ? NoContent() : NotFound();
    }
}

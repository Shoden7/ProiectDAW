using System.ComponentModel.DataAnnotations;

namespace TekkenStats.Application.DTOs.Auth;

public class RegisterDto
{
    [Required, MinLength(3), MaxLength(30)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? LinkedPolarisId { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class LinkPlayerDto
{
    [Required]
    public string PolarisId { get; set; } = string.Empty;
}

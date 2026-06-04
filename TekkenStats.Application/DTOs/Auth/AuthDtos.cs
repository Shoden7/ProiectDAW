using System.ComponentModel.DataAnnotations;

namespace TekkenStats.Application.DTOs.Auth;

public record RegisterDto(
    [Required, MinLength(3), MaxLength(50)] string Username,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password
);

public record LoginDto(
    [Required] string Email,
    [Required] string Password
);

public record AuthResponseDto(
    string Token,
    string Username,
    int UserId,
    DateTime ExpiresAt
);

public record LinkPolarisIdDto(
    [Required] string PolarisId
);

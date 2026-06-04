using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TekkenStats.Application.DTOs.Auth;
using TekkenStats.Application.Interfaces;
using TekkenStats.Domain.Entities;

namespace TekkenStats.Application.Services;

public class AuthService(IUserRepository users, IConfiguration config)
{
    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        if (await users.GetByEmailAsync(dto.Email, ct) is not null) return null;
        if (await users.GetByUsernameAsync(dto.Username, ct) is not null) return null;

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email.ToLowerInvariant(),
            PasswordHash = HashPassword(dto.Password)
        };

        await users.AddAsync(user, ct);
        await users.SaveChangesAsync(ct);
        return GenerateToken(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(dto.Email.ToLowerInvariant(), ct);
        if (user is null) return null;
        if (!VerifyPassword(dto.Password, user.PasswordHash)) return null;
        return GenerateToken(user);
    }

    public async Task<bool> LinkPolarisIdAsync(int userId, string polarisId, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId, ct);
        if (user is null) return false;
        user.LinkedPolarisId = polarisId;
        await users.SaveChangesAsync(ct);
        return true;
    }

    private AuthResponseDto GenerateToken(User user)
    {
        var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddDays(7);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return new AuthResponseDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Username,
            user.Id,
            expiry);
    }

    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;
        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] storedBytes = Convert.FromBase64String(parts[1]);
        byte[] incoming = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(incoming, storedBytes);
    }
}

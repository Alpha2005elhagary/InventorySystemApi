using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventorySystemApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace InventorySystemApi.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(ApplicationUser user, IList<string> roles)
    {
        var keyString = _config["Jwt:Key"] ?? "SuperSecretKeyForInventorySystemApiProject2026!#SecureJWTBearerTokenGeneratorKey";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName ?? user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expiryDays = int.TryParse(_config["Jwt:ExpiryInDays"], out var days) ? days : 7;
        var expires = DateTime.UtcNow.AddDays(expiryDays);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "InventorySystemApi",
            audience: _config["Jwt:Audience"] ?? "InventorySystemClients",
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

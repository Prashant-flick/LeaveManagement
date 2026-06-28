using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Infrastructure.Services;
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration config, ILogger<JwtTokenService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public string GenerateToken(User user, List<string> roles, int employeeId)
    {
        _logger.LogInformation(
            "Generating JWT for UserId: {UserId}, Email: {Email}",
            user.Id,
            user.Email);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        _logger.LogInformation(
            "EmployeeId for token generation: {EmployeeId}",
            employeeId);

        if (employeeId > 0)
        {
            claims.Add(new Claim("EmployeeId", employeeId.ToString()));
        }

        foreach (var role in roles)
        {
            _logger.LogDebug(
                "Adding role claim: {Role} for UserId: {UserId}",
                role,
                user.Id);

            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var keyString = _config["Jwt:Key"];

        if (string.IsNullOrEmpty(keyString))
            throw new Exception("JWT Key is missing in configuration");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(keyString)
        );

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var expiryMinutes = int.Parse(
            _config["Jwt:ExpiryMinutes"] ?? "60"
        );

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation(
            "JWT generated successfully for UserId: {UserId} with roles: {Roles}",
            user.Id,
            string.Join(",", roles));

        return tokenString;
    }

    public string GenerateRefreshToken()
    {
        var random = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(random);

        return Convert.ToBase64String(random);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var keyString = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(keyString))
            throw new Exception("JWT Key is missing in configuration");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = _config["Jwt:Audience"],
            ValidIssuer = _config["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(keyString)
            ),
            ValidateLifetime = false // Disable lifetime validation to read expired token claims
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid security token algorithm or type.");
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating expired token");
            return null;
        }
    }
}
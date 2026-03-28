using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartLeads.Domain.Models;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Utilities.Identity;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user, Domain.Enums.UserRole? role = null, Guid? companyId = null)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not found");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add CompanyId claim if provided (for company context)
        if (companyId.HasValue && companyId != Guid.Empty)
        {
            claims.Add(new Claim("CompanyId", companyId.Value.ToString()));
        }

        // Add FirstName and LastName claims
        if (!string.IsNullOrEmpty(user.FirstName))
        {
            claims.Add(new Claim("FirstName", user.FirstName));
        }
        if (!string.IsNullOrEmpty(user.LastName))
        {
            claims.Add(new Claim("LastName", user.LastName));
        }

        // Add Role claim - use provided role or default to User
        var userRole = role ?? Domain.Enums.UserRole.User;
        claims.Add(new Claim(ClaimTypes.Role, userRole.ToString()));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

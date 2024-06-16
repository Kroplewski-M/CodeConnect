using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DomainLayer.Entities.APIClasses;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace ApplicationLayer.APIServices;

public class TokenGenerationService(IOptions<JwtSettings>jwtSettings)
{
    public string GenerateJwtToken(IEnumerable<Claim> claims, DateTime expireAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer: jwtSettings.Value.Issuer,
            audience: jwtSettings.Value.Audience,
            claims: claims, 
            expires: expireAt, 
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
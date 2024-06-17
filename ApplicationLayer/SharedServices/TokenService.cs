using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DomainLayer.Entities.APIClasses;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace ApplicationLayer.APIServices;

public class TokenService(IOptions<JwtSettings>jwtSettings)
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
    public IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        return null;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSettings.Value.Key);
        try
        {
            var tokenValidation = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Value.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Value.Audience,
                ValidateLifetime = true
            };
            SecurityToken validateToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidation, out validateToken);
            return principal;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationLayer.DTO_s;
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
            claims: claims.Where(x=>x.Type != "aud"), 
            expires: expireAt, 
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public List<Claim>? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Key));
        try
        {
            var tokenValidation = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Value.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Value.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };
            SecurityToken validateToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidation, out validateToken);
            return principal.Claims.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(DateTime.Now);
            Console.WriteLine($"Token validation failed: {ex.Message}");
            return null;
        }
    }

    public TokenResponse RefreshToken(string token)
    {
       var result =  ValidateToken(token);
       return result == null? new TokenResponse("") : new TokenResponse(GenerateJwtToken(result, DateTime.Now.AddMinutes(60)));
    }
}
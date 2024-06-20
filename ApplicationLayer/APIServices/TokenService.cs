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
    public string? GenerateJwtToken(IEnumerable<Claim> claims, DateTime expireAt)
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
    public ClaimsPrincipalResponse ValidateToken(string token)
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
            return new ClaimsPrincipalResponse(true,principal);
        }
        catch (Exception ex)
        {
            return new ClaimsPrincipalResponse(false,new ClaimsPrincipal());
        }
    }

    public TokenResponse RefreshToken(string token)
    {
       var principal =  ValidateToken(token);
       return principal.ClaimsPrincipal.Claims.ToList().Any()? new TokenResponse(GenerateJwtToken(principal.ClaimsPrincipal.Claims, DateTime.Now.AddMinutes(60))) :  new TokenResponse("") ;
    }
}
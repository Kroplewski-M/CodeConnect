using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.APIClasses;
using DomainLayer.Entities.Auth;
using DomainLayer.Generics;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace ApplicationLayer.APIServices;

public class TokenService(IOptions<JwtSettings> jwtSettings,ApplicationDbContext context,UserManager<ApplicationUser>userManager) : ITokenService
{
    public string? GenerateJwtToken(IEnumerable<Claim> claims, DateTime expireAt)
    {
        var enumerable = claims as Claim[] ?? claims.ToArray();
        if(enumerable.Length == 0)
            return string.Empty;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer: jwtSettings.Value.Issuer,
            audience: jwtSettings.Value.Audience,
            claims: enumerable.Where(x => x.Type != "aud"),
            expires: expireAt,
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public ClaimsPrincipalResponse ValidateToken(string token)
    {
        if(string.IsNullOrWhiteSpace(token))
            return new ClaimsPrincipalResponse(false, new ClaimsPrincipal());
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
            return new ClaimsPrincipalResponse(true, principal);
        }
        catch
        {
            return new ClaimsPrincipalResponse(false, new ClaimsPrincipal());
        }
    }

    public async Task<AuthResponse> RefreshUserTokens(string userName, string refreshToken)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user == null)
            return new AuthResponse(false,"","","Couldn't find user when refreshing token");
        var refresh = context.RefreshUserAuths.FirstOrDefault(x => x.UserId == user.Id);
        if (refresh != null && refresh?.RefreshToken == refreshToken)
        {
            var userClaims = Generics.GetClaimsForUser(user);
            var token = GenerateJwtToken(userClaims, DateTime.UtcNow.AddMinutes(Consts.Tokens.AuthTokenMins));
            var newRefreshToken = GenerateJwtToken(userClaims, DateTime.UtcNow.AddMinutes(Consts.Tokens.RefreshTokenMins));
            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(refreshToken) && !string.IsNullOrWhiteSpace(newRefreshToken))
            {
                try
                {
                    refresh.RefreshToken = newRefreshToken;
                    await context.SaveChangesAsync();
                }
                catch
                {
                    return new AuthResponse(false,"","","Error occurred authenticating refresh token");
                }
                return new AuthResponse(true, token, refreshToken, "success");
            }
        }
        return new AuthResponse(false,"","","Error occurred authenticating refresh token");
    }
}

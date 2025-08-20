using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.ExtensionClasses;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.APIClasses;
using DomainLayer.Entities.Auth;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebApiApplicationLayer.Services;

public class TokenService(IOptions<JwtSettings> jwtSettings,ApplicationDbContext context,UserManager<ApplicationUser>userManager) : ITokenService
{
    public string? GenerateJwtToken(IEnumerable<Claim> claims, DateTime expireAt, Consts.TokenType tokenType = Consts.TokenType.Access)
    {
        var allClaims = claims.ToList();
        if(!allClaims.Any())
            return string.Empty;
        allClaims.Add(new Claim(Consts.Tokens.Type, tokenType.ToString()));
        var enumerable = allClaims;
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

    public async Task<AuthResponse> RefreshUserTokens(string refreshToken, string deviceId)
    {
        if(!Guid.TryParse(deviceId, out Guid deviceGuid))
            return new AuthResponse(false,"","","Invalid device id");
        var res = ValidateToken(refreshToken);
        if(!res.Flag)
            return new AuthResponse(false,"","","Invalid token");
            
        //make sure token is a refresh token
        var tokenType = res?.ClaimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == Consts.Tokens.Type)?.Value?.ToString();
        if(tokenType != nameof(Consts.TokenType.Refresh))
            return new AuthResponse(false,"","","Token is not a refresh token");
            
        var username = res?.ClaimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == Consts.ClaimTypes.UserName)?.Value?.ToString();
        if(string.IsNullOrWhiteSpace(username))
            return new AuthResponse(false,"","","error refreshing token");
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            return new AuthResponse(false,"","","Couldn't find user when refreshing token");
        var refresh = context.RefreshUserAuths.FirstOrDefault(x=> x.UserId == user.Id && x.Expires > DateTime.UtcNow && x.DeviceId == deviceGuid);
        if (refresh != null && refresh?.RefreshToken == refreshToken)
        {
            var userClaims = user.GetClaimsForUser();
            var token = GenerateJwtToken(userClaims, DateTime.UtcNow.AddMinutes(Consts.Tokens.AuthTokenMins));
            var newRefreshToken = GenerateJwtToken(userClaims, DateTime.UtcNow.AddMinutes(Consts.Tokens.RefreshTokenMins), Consts.TokenType.Refresh);
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
                return new AuthResponse(true, token, newRefreshToken, "success");
            }
        }
        return new AuthResponse(false,"","","Error occurred authenticating refresh token");
    }
}

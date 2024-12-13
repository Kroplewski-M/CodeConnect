using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationLayer.DTO_s;
using DomainLayer.Constants;
using DomainLayer.Entities.APIClasses;
using DomainLayer.Entities.Auth;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace ApplicationLayer.APIServices;

public class TokenService(IOptions<JwtSettings> jwtSettings,ApplicationDbContext context,UserManager<ApplicationUser>userManager)
{
    public string? GenerateJwtToken(IEnumerable<Claim> claims, DateTime expireAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer: jwtSettings.Value.Issuer,
            audience: jwtSettings.Value.Audience,
            claims: claims.Where(x => x.Type != "aud"),
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
            List<Claim>userClaims = new List<Claim>()
            {
                new Claim(Constants.ClaimTypes.FirstName, user?.FirstName ?? ""),
                new Claim(Constants.ClaimTypes.LastName, user?.LastName ?? ""),
                new Claim(Constants.ClaimTypes.UserName, user?.UserName ?? ""),
                new Claim(Constants.ClaimTypes.Bio, user?.Bio ?? ""),
                new Claim(Constants.ClaimTypes.Email, user?.Email ?? ""),
                new Claim(Constants.ClaimTypes.Dob, user.DOB.ToString(CultureInfo.InvariantCulture)),
                new Claim(Constants.ClaimTypes.CreatedAt, user.CreatedAt.ToString(CultureInfo.InvariantCulture)),
                new Claim(Constants.ClaimTypes.ProfileImg, user?.ProfileImageUrl ?? ""),
                new Claim(Constants.ClaimTypes.BackgroundImg, user?.BackgroundImageUrl ?? ""),
                new Claim(Constants.ClaimTypes.GithubLink, user?.GithubLink ?? ""),
                new Claim(Constants.ClaimTypes.WebsiteLink, user?.WebsiteLink ?? ""),
            };
            var token = GenerateJwtToken(userClaims, DateTime.UtcNow.AddMinutes(Constants.Tokens.AuthTokenMins));
            var newRefreshToken = GenerateJwtToken(userClaims, DateTime.UtcNow.AddMinutes(Constants.Tokens.RefreshTokenMins));
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(refreshToken))
            {
                return new AuthResponse(true, token, refreshToken, "success");
            }
        }
        return new AuthResponse(false,"","","Error occurred authenticating refresh token");
    }
}

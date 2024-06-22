using System.Globalization;
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.APIServices;

public class AuthenticateService(UserManager<ApplicationUser>userManager,
    TokenService tokenGenerationService) : IAuthenticateService
{
    public async Task<AuthResponse> CreateUser(RegisterFormViewModel registerForm)
    {
        var user = new ApplicationUser
        {
            UserName = registerForm.Email,
            FirstName = registerForm.FirstName,
            LastName = registerForm.LastName,
            Email = registerForm.Email,
            DOB = registerForm.DOB,
            CreatedAt = DateTime.Now,
        };
        var result = await userManager.CreateAsync(user, registerForm.Password);
        if (result.Succeeded)
        {
            var userClaims = GetClaimsForUser(user);
            return GenerateAuthResponse(userClaims);
        }
        return new AuthResponse(false,"", "");
    }

    public async Task<AuthResponse> LoginUser(LoginFormViewModel loginForm)
    {
        var user = await userManager.FindByEmailAsync(loginForm.Email);
        if (user != null)
        {
            var userClaims = GetClaimsForUser(user);
            return GenerateAuthResponse(userClaims);
        }
        return new AuthResponse(false, "", "");
    }

    private List<Claim> GetClaimsForUser(ApplicationUser user)
    {
        return
        [
            new Claim("FirstName", user.FirstName ?? ""),
            new Claim("LastName", user.LastName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim("DOB", user.DOB.ToString(CultureInfo.InvariantCulture)),
            new Claim("CreatedAt", user.CreatedAt.ToString(CultureInfo.InvariantCulture)),
            new Claim("ProfileImg", user.ProfileImageUrl ?? ""),
            new Claim("BackgroundImg", user.BackgroundImageUrl ?? "")
        ];
    }

    private AuthResponse GenerateAuthResponse(List<Claim> userClaims)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(60);
        var token = tokenGenerationService.GenerateJwtToken(userClaims,expiresAt);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(30);
        var refreshToken = tokenGenerationService.GenerateJwtToken(userClaims,refreshExpiresAt);
        return new AuthResponse(true, token, refreshToken);
    }
}
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
    public async Task<AuthResponse> CreateUser(RegisterForm registerForm)
    {
        var user = new ApplicationUser
        {
            UserName = registerForm.Email,
            FirstName = registerForm.FirstName,
            LastName = registerForm.LastName,
            Email = registerForm.Email,
            DOB = registerForm.DOB,
            CreatedAt = DateOnly.FromDateTime(DateTime.Now),
        };
        var result = await userManager.CreateAsync(user, registerForm.Password);
        if (result.Succeeded)
        {
            var userClaims = GetClaimsForUser(user);
            return GenerateAuthResponse(userClaims);
        }
        string error = result.Errors.Select(x => x.Description).FirstOrDefault() ?? "";
        return new AuthResponse(false,"", "",error);
    }

    public async Task<AuthResponse> LoginUser(LoginForm loginForm)
    {
        var user = await userManager.FindByEmailAsync(loginForm.Email);
        if (user != null)
        {
            var correctPassword = await userManager.CheckPasswordAsync(user, loginForm.Password);
            if (correctPassword)
            {
                var userClaims = GetClaimsForUser(user);
                return GenerateAuthResponse(userClaims);
            }
        }
        return new AuthResponse(false, "", "","Incorrect Email or Password");
    }

    public Task<AuthResponse> LogoutUser()
    {
        throw new NotImplementedException();
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
        return new AuthResponse(true, token, refreshToken, "Auth successful");
    }
}
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
    public async Task<RegisterResponse> CreateUser(RegisterFormViewModel registerForm)
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
            var claims = new List<Claim>
            {
                new Claim("FirstName", registerForm.FirstName),
                new Claim("LastName", registerForm.LastName),
                new Claim(ClaimTypes.Email, registerForm.Email),
                new Claim("DOB", registerForm.DOB.ToString(CultureInfo.InvariantCulture)),
                new Claim("CreatedAt", user.CreatedAt.ToString(CultureInfo.InvariantCulture)),
            };
            var expiresAt = DateTime.UtcNow.AddMinutes(60);
            var token = tokenGenerationService.GenerateJwtToken(claims,expiresAt);
            var refreshExpiresAt = DateTime.UtcNow.AddDays(30);
            var refreshToken = tokenGenerationService.GenerateJwtToken(claims,refreshExpiresAt);
            return new RegisterResponse(true,token,refreshToken);
        }
        else
        {
            return new RegisterResponse(false,"", "");
        }
    }

    public async Task<RegisterResponse> LoginUser()
    {
        throw new NotImplementedException();
    }
}
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.APIServices;

public class AuthenticateService(UserManager<ApplicationUser>userManager) : IAuthenticateService
{
    public async Task<ServiceResponse> CreateUser(RegisterFormViewModel registerForm)
    {
        var user = new ApplicationUser
        {
            FirstName = registerForm.FirstName,
            LastName = registerForm.LastName,
            Email = registerForm.Email,
            DOB = registerForm.DOB,
            CreatedAt = DateTime.Now,
        };
        var result = await userManager.CreateAsync(user, registerForm.Password);
        if (result.Succeeded)
        {
            return new ServiceResponse(true, "User created successfully");
        }
        else
        {
            return new ServiceResponse(false, "Failed to create user");
        }
    }

    public async Task<ServiceResponse> LoginUser()
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResponse> LogoutUser()
    {
        throw new NotImplementedException();
    }
}
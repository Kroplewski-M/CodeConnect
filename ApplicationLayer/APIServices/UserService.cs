using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.APIServices;

public class UserService(UserManager<ApplicationUser>userManager) : IUserService
{
    public async Task<ServiceResponse> UpdateUserDetails(EditProfileForm editProfileForm)
    {
       var user =  await userManager.FindByNameAsync(editProfileForm.Username);
       if (user == null)
           return new ServiceResponse(false, "");
       user.FirstName = editProfileForm.FirstName;
       user.LastName = editProfileForm.LastName;
       user.Bio = editProfileForm.Bio;
       user.GithubLink = editProfileForm.GithubLink;
       user.WebsiteLink = editProfileForm.WebsiteLink;

       await userManager.UpdateAsync(user);
       //update token for user
       return new ServiceResponse(true, "");
    }
    
}
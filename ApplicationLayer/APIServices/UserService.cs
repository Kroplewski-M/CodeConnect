using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
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
       return new ServiceResponse(true, "Updated User Successfully");
    }

    public async Task<UserDetails?> GetUserDetails(string username)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            return null;
        if (string.IsNullOrEmpty(user.ProfileImageUrl))
            user.ProfileImageUrl = Constants.ProfilleDefaults.ProfileImg;
        if (string.IsNullOrEmpty(user.BackgroundImageUrl))
            user.BackgroundImageUrl = Constants.ProfilleDefaults.BackgroundImg;
        return new UserDetails(user.FirstName ?? "", user.LastName ?? "", user.UserName ?? "", user.Email ?? "", user.ProfileImageUrl ?? "",
            user.BackgroundImageUrl ?? "", user.GithubLink ?? "",
            user.WebsiteLink ?? "", user.DOB, user.CreatedAt,user.Bio ?? "");
    }
}
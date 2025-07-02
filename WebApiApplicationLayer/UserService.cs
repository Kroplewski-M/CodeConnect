using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.DbEnts;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.User;
using DomainLayer.Helpers;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ApplicationLayer.APIServices;

public class UserService(UserManager<ApplicationUser>userManager, ApplicationDbContext context,
    IMemoryCache memoryCache) : IUserService
{
    public async Task<ServiceResponse> UpdateUserDetails(EditProfileForm editProfileForm)
    {
        var validator = new EditProfileValidator();
        var result = await validator.ValidateAsync(editProfileForm);
        if (!result.IsValid)
            return new ServiceResponse(false, "invalid request"); 
        
        var user =  await userManager.FindByNameAsync(editProfileForm.Username);
       if (user == null)
           return new ServiceResponse(false, "");
       user.FirstName = editProfileForm.FirstName;
       user.LastName = editProfileForm.LastName;
       user.Bio = editProfileForm.Bio;
       user.GithubLink = editProfileForm.GithubLink;
       user.WebsiteLink = editProfileForm.WebsiteLink;
       user.DOB = editProfileForm.Dob;

       await userManager.UpdateAsync(user);
       return new ServiceResponse(true, "Updated User Successfully");
    }

    public async Task<UserDetails?> GetUserDetails(string username)
    {
        if (string.IsNullOrEmpty(username))
            return null;
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            return null;
        
        var profileImg = Helpers.GetUserImgUrl(user.ProfileImage, Consts.ImageType.ProfileImages);
        var backgroundImg = Helpers.GetUserImgUrl(user.BackgroundImage, Consts.ImageType.BackgroundImages);
        
        return new UserDetails(user.FirstName ?? "", user.LastName ?? "", user.UserName ?? "", user.Email ?? "", profileImg,
            backgroundImg, user.GithubLink ?? "",
            user.WebsiteLink ?? "", user.DOB, user.CreatedAt,user.Bio ?? "");
    }

    public async Task<UserInterestsDto> GetUserInterests(string username)
    {
        if (string.IsNullOrEmpty(username))
            return new UserInterestsDto(false,"user not found", null);
        var user = await userManager.FindByNameAsync(username);
        if (user != null)
        {
            var interests = context.UserInterests.AsNoTracking()
                .Where(x=> x.TechInterest != null)
                .Include(x => x.TechInterest)
                .Include(x=>x.TechInterest!.Interest)
                .Where(x => x.UserId == user.Id)
                .ToList()
                .Select(x=> new TechInterestsDto(x.TechInterestId,x.TechInterest!.Id,x.TechInterest.Interest!.Name,x.TechInterest.Name))
                .ToList();
            return new UserInterestsDto(true, "user interests fetched successfully", interests.Any() ? interests : null);
        }
        return new UserInterestsDto(false,"user not found", null);
    }

    public async Task<ServiceResponse> UpdateUserInterests(UpdateTechInterestsDto interests)
    {
        if (string.IsNullOrEmpty(interests.Username))
            return new ServiceResponse(false, "user not found");
        var user = await userManager.FindByNameAsync(interests.Username ?? "");
        if (user != null)
        {
            var oldInterests = context.UserInterests.Include(x => x.TechInterest)
                .Where(x => x.UserId == user.Id).ToList();
            if(oldInterests.Count > 0)
                context.UserInterests.RemoveRange(oldInterests);
            var newInterests = interests.Interests.Select(x => new UserInterests
            {
                UserId = user.Id,
                TechInterestId = x.Id,
            }).ToList();
            if (newInterests.Any())
            {
                context.UserInterests.AddRange(newInterests);
            }
            await context.SaveChangesAsync();
            return new ServiceResponse(true, "Interests updated successfully");
        }
        return new ServiceResponse(false, "User not found");
    }

    public async Task<List<TechInterestsDto>> GetAllInterests()
    {
        var cacheKey = Consts.CacheKeys.AllInterests;
        if (!memoryCache.TryGetValue(cacheKey, out List<TechInterestsDto>? techInterests))
        {
            var interests = await context.TechInterests
                .Include(x => x.Interest)
                .ToListAsync();
            
            techInterests = interests.Where(x=> x.Interest != null)
                .Select(x => new TechInterestsDto(x.Id, x.Interest!.Id, x.Interest.Name, x.Name))
                .ToList();
            memoryCache.Set(cacheKey, techInterests, TimeSpan.FromHours(24));
        }
        return techInterests ?? new List<TechInterestsDto>();
    }
}
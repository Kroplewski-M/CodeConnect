using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.DbEnts;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
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
            user.ProfileImageUrl = Constants.ProfileDefaults.ProfileImg;
        if (string.IsNullOrEmpty(user.BackgroundImageUrl))
            user.BackgroundImageUrl = Constants.ProfileDefaults.BackgroundImg;
        return new UserDetails(user.FirstName ?? "", user.LastName ?? "", user.UserName ?? "", user.Email ?? "", user.ProfileImageUrl ?? "",
            user.BackgroundImageUrl ?? "", user.GithubLink ?? "",
            user.WebsiteLink ?? "", user.DOB, user.CreatedAt,user.Bio ?? "");
    }

    public async Task<UserInterestsDto> GetUserInterests(string username)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user != null)
        {
            var interests = context.UserInterests.Where(x=> x.TechInterest != null)
                .Include(x => x.TechInterest).Include(x=>x.TechInterest.Interest)
                .Where(x => x.UserId == user.Id)
                .ToList()
                .Select(x=> new TechInterestsDto(x.Id,x.TechInterest.Interest.Id,x.TechInterest.Interest.Name,x.TechInterest.Name))
                .ToList();
            return new UserInterestsDto(true, "user interests fetched successfully", interests.Any() ? interests : null);
        }
        return new UserInterestsDto(false,"user not found", null);
    }

    public async Task<ServiceResponse> UpdateUserInterests(string? username, List<TechInterestsDto> userInterests)
    {
        var user = await userManager.FindByNameAsync(username ?? "");
        if (user != null)
        {
            var interests = context.UserInterests.Include(x => x.TechInterest)
                .Where(x => x.UserId == user.Id).ToList();
            context.UserInterests.RemoveRange(interests);
            var newInterests = userInterests.Select(x => new UserInterests
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
        var cacheKey = Constants.CacheKeys.AllInterests;
        if (!memoryCache.TryGetValue(cacheKey, out List<TechInterestsDto> techInterests))
        {
            var interests = await context.TechInterests
                .Include(x => x.Interest)
                .ToListAsync();
            techInterests = interests
                .Select(x => new TechInterestsDto(x.Id, x.Interest.Id, x.Interest.Name, x.Name))
                .ToList();
            memoryCache.Set(cacheKey, techInterests, TimeSpan.FromHours(24));
        }
        return techInterests;
    }
}
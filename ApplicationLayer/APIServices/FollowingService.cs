using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.User;
using DomainLayer.Helpers;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApplicationLayer.APIServices;

public class FollowingService(UserManager<ApplicationUser>userManager, ApplicationDbContext context): IFollowingService
{
    public async Task<FollowerCount> GetUserFollowersCount(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if(user == null)
            throw new UnauthorizedAccessException($"User {userId} not found");
        var following = context.FollowUsers.Count(x => x.FollowerUserId == user.Id);
        var followers = context.FollowUsers.Count(x => x.FollowedUserId == user.Id);
        return new FollowerCount(followers,following);
    }
    public async Task<ServiceResponse> FollowUser(FollowRequestDto followRequest)
    {
        if (string.IsNullOrWhiteSpace(followRequest.CurrentUsername) ||
            string.IsNullOrWhiteSpace(followRequest.TargetUsername))
            return new ServiceResponse(false, "Error occured while following user");
        var currentUser = await userManager.FindByNameAsync(followRequest.CurrentUsername);
        var targetUser = await userManager.FindByNameAsync(followRequest.TargetUsername);
        if(currentUser == null || targetUser == null)
            return new ServiceResponse(false, "A user could not be found");
        var alreadyFollowing = context.FollowUsers.Any(x =>
            x.FollowerUserId == currentUser.Id && x.FollowedUserId == targetUser.Id);
        if (alreadyFollowing)
            return new ServiceResponse(false, "User already followed");
        
        var newFollow = new Followers()
        {
            FollowerUserId = currentUser.Id,
            FollowedUserId = targetUser.Id
        };
        await context.AddAsync(newFollow);
        await context.SaveChangesAsync();
        return new ServiceResponse(true, "Followed Successfully");
    }

    public async Task<ServiceResponse> UnfollowUser(FollowRequestDto unFollowRequest)
    {
        if (string.IsNullOrWhiteSpace(unFollowRequest.CurrentUsername) ||
            string.IsNullOrWhiteSpace(unFollowRequest.TargetUsername))
            return new ServiceResponse(false, "Error occured while unfollowing user");
        
        var currentUser = await userManager.FindByNameAsync(unFollowRequest.CurrentUsername);
        var targetUser = await userManager.FindByNameAsync(unFollowRequest.TargetUsername);
        if(currentUser == null || targetUser == null)
            return new ServiceResponse(false, "A user could not be found");
        var unfollow = context.FollowUsers.FirstOrDefault(x=> x.FollowerUserId == currentUser.Id && x.FollowedUserId == targetUser.Id);
        if(unfollow == null)
            return new ServiceResponse(false, "A user could not be found");
        context.FollowUsers.Remove(unfollow);
        await context.SaveChangesAsync();
        return new ServiceResponse(true, "UnFollowed Successfully");
    }

    public async Task<bool> IsUserFollowing(FollowRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentUsername) ||
            string.IsNullOrWhiteSpace(request.TargetUsername))
            return false;
        var currentUser = await userManager.FindByNameAsync(request.CurrentUsername);
        var targetUser = await userManager.FindByNameAsync(request.TargetUsername);
        if(currentUser == null || targetUser == null)
            return false;
        return context.FollowUsers.Any(x=> x.FollowerUserId == currentUser.Id && x.FollowedUserId == targetUser.Id);
    }
    public async Task<List<UserBasicDto>> GetUserFollowers(string username, int skip, int take)
    {
        if (string.IsNullOrWhiteSpace(username))
            return new List<UserBasicDto>();
        var user = await userManager.FindByNameAsync(username);
        if(user == null || user?.UserName != username)
            return new List<UserBasicDto>();
        var users = context.FollowUsers.AsNoTracking()
            .Where(x => x.FollowedUserId == user.Id && x.Follower != null)
            .Include(x=> x.Follower)
            .OrderByDescending(x=> x.Follower!.UserName)
            .Skip(skip)
            .Take(take)
            .ToList()
            .Where(x => x.Follower is { UserName: not null })
            .Select(x=> new UserBasicDto(x.Follower!.UserName!,x.Follower.Bio!,Helpers.GetUserImgUrl(x.Follower.ProfileImage!, Consts.ImageType.ProfileImages)))
            .ToList();
        return users;
    }

    public async Task<List<UserBasicDto>> GetUserFollowing(string username, int skip, int take)
    {
        if (string.IsNullOrWhiteSpace(username))
            return new List<UserBasicDto>();
        var user = await userManager.FindByNameAsync(username);
        if(user == null|| user?.UserName != username)
            return new List<UserBasicDto>();
        var users = context.FollowUsers.AsNoTracking()
            .Where(x => x.FollowerUserId == user.Id)
            .Include(x=> x.Followed)
            .OrderByDescending(x=> x.Followed!.UserName)
            .Skip(skip)
            .Take(take)
            .ToList()
            .Select(x=> new UserBasicDto(x.Followed!.UserName!,x.Followed.Bio!,Helpers.GetUserImgUrl(x.Followed.ProfileImage!, Consts.ImageType.ProfileImages)))
            .ToList();
        return users;
    }
}
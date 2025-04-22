using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.User;
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
        var currentUser = await userManager.FindByNameAsync(followRequest.CurrentUsername);
        var targetUser = await userManager.FindByNameAsync(followRequest.TargetUsername);
        if(currentUser == null || targetUser == null)
            return new ServiceResponse(false, "A user could not be found");
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
        var currentUser = await userManager.FindByNameAsync(request.CurrentUsername);
        var targetUser = await userManager.FindByNameAsync(request.TargetUsername);
        if(currentUser == null || targetUser == null)
            return false;
        return context.FollowUsers.Any(x=> x.FollowerUserId == currentUser.Id && x.FollowedUserId == targetUser.Id);
    }
    public async Task<List<UserBasicDto>> GetUserFollowers(string username)
    {
        var user = await userManager.FindByNameAsync(username);
        if(user == null || user?.UserName != username)
            return new List<UserBasicDto>();
        var users = context.FollowUsers.Where(x => x.FollowedUserId == user.Id)
            .Include(x=> x.Follower)
            .ToList()
            .Where(x => x.Follower is { UserName: not null })
            .Select(x=> new UserBasicDto(x.Follower!.UserName!,x.Follower.Bio!,x.Follower.ProfileImage! ))
            .ToList();
        return users;
    }

    public async Task<List<UserBasicDto>> GetUserFollowing(string username)
    {
        var user = await userManager.FindByNameAsync(username);
        if(user == null|| user?.UserName != username)
            return new List<UserBasicDto>();
        var users = context.FollowUsers.Where(x => x.FollowerUserId == user.Id)
            .Include(x=> x.Followed)
            .ToList()
            .Where(x => x.Followed is { UserName: not null })
            .Select(x=> new UserBasicDto(x.Followed!.UserName!,x.Followed.Bio!,x.Followed.ProfileImage! ))
            .ToList();
        return users;
    }
}
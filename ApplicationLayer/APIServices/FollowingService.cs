using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.User;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.APIServices;

public class FollowingService(UserManager<ApplicationUser>userManager, ApplicationDbContext context): IFollowingService
{
    public async Task<FollowerCount> GetUserFollowers(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if(user == null)
            throw new UnauthorizedAccessException($"User {userId} not found");
        var following = context.FollowUsers.Count(x => x.FollowerUserId == user.Id);
        var followers = context.FollowUsers.Count(x => x.FollowedUserId == user.Id);
        return new FollowerCount(following, followers);
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
}
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
        var unfollow = new Followers()
        {
            FollowerUserId = currentUser.Id,
            FollowedUserId = targetUser.Id
        };
        context.Remove(unfollow);
        await context.SaveChangesAsync();
        return new ServiceResponse(true, "UnFollowed Successfully");
    }

}
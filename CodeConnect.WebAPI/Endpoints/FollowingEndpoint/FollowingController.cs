using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeConnect.WebAPI.Endpoints.FollowingEndpoint;

[Route("api/[controller]")]
[ApiController]
public class FollowingController(IFollowingService followingService, UserManager<ApplicationUser>userManager) : ControllerBase
{
    [Authorize]
    [HttpPost("FollowUser")]
    public async Task<IActionResult> FollowUser(FollowRequestDto followRequest)
    {
        var username = User.FindFirst(Constants.ClaimTypes.UserName)?.Value;
        if(username == null || username != followRequest.CurrentUsername)
            return Unauthorized();
        var response = await followingService.FollowUser(followRequest);
        return Ok(response);
    }
    [Authorize]
    [HttpPost("UnFollowUser")]
    public async Task<IActionResult> UnFollowUser(FollowRequestDto unFollowRequest)
    {
        var username = User.FindFirst(Constants.ClaimTypes.UserName)?.Value;
        if(username == null || username != unFollowRequest.CurrentUsername)
            return Unauthorized();
        var response = await followingService.UnfollowUser(unFollowRequest);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("UserFollowing")]
    public async Task<IActionResult> UserFollowing(string username)
    {
        var user = await userManager.FindByNameAsync(username);
        if(user == null || user.UserName != username)
            return Unauthorized();
        return Ok(await followingService.GetUserFollowers(user.Id));
    }

    [Authorize]
    [HttpGet("IsUserUnfollowing")]
    public async Task<IActionResult> IsUserUnfollowing(string currentUsername, string targetUsername)
    {
        var request = new FollowRequestDto(currentUsername, targetUsername);
        return Ok(await followingService.IsUserFollowing(request));
    }
}
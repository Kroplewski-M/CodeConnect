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
        var username = User.FindFirst(Consts.ClaimTypes.UserName)?.Value;
        if(username == null || username != followRequest.CurrentUsername)
            return Unauthorized();
        var response = await followingService.FollowUser(followRequest);
        return Ok(response);
    }
    [Authorize]
    [HttpPost("UnFollowUser")]
    public async Task<IActionResult> UnFollowUser(FollowRequestDto unFollowRequest)
    {
        var username = User.FindFirst(Consts.ClaimTypes.UserName)?.Value;
        if(username == null || username != unFollowRequest.CurrentUsername)
            return Unauthorized();
        var response = await followingService.UnfollowUser(unFollowRequest);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("UserFollowersCount")]
    public async Task<IActionResult> UserFollowersCount(string username)
    {
        var user = await userManager.FindByNameAsync(username);
        if(user == null || user.UserName != username)
            return Unauthorized();
        return Ok(await followingService.GetUserFollowersCount(user.Id));
    }

    [Authorize]
    [HttpGet("IsUserUnfollowing")]
    public async Task<IActionResult> IsUserUnfollowing(string currentUsername, string targetUsername)
    {
        var request = new FollowRequestDto(currentUsername, targetUsername);
        return Ok(await followingService.IsUserFollowing(request));
    }

    [Authorize]
    [HttpGet("GetUserFollowers")]
    public async Task<IActionResult> GetUserFollowers(string username)
    {
        return Ok(await followingService.GetUserFollowers(username));
    }
    [Authorize]
    [HttpGet("GetUserFollowing")]
    public async Task<IActionResult> GetUserFollowing(string username)
    {
        if(string.IsNullOrEmpty(username))
            return Unauthorized();
        return Ok(await followingService.GetUserFollowing(username));
    }
    
}
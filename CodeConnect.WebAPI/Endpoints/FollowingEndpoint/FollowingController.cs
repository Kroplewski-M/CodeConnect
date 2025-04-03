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
        if(string.IsNullOrWhiteSpace(followRequest.CurrentUsername) || string.IsNullOrWhiteSpace(followRequest.TargetUsername))
            return BadRequest();
        
        var username = User.FindFirst(Consts.ClaimTypes.UserName)?.Value;
        
        if(string.IsNullOrWhiteSpace(username)  || username != followRequest.CurrentUsername)
            return BadRequest();
        
        var response = await followingService.FollowUser(followRequest);
        return Ok(response);
    }
    [Authorize]
    [HttpPost("UnFollowUser")]
    public async Task<IActionResult> UnFollowUser(FollowRequestDto unFollowRequest)
    {
        if(string.IsNullOrWhiteSpace(unFollowRequest.CurrentUsername) || string.IsNullOrWhiteSpace(unFollowRequest.TargetUsername))
            return BadRequest();
        
        var username = User.FindFirst(Consts.ClaimTypes.UserName)?.Value;
        
        if(string.IsNullOrWhiteSpace(username) || username != unFollowRequest.CurrentUsername)
            return BadRequest();
        
        var response = await followingService.UnfollowUser(unFollowRequest);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("UserFollowersCount")]
    public async Task<IActionResult> UserFollowersCount(string username)
    {
        if(string.IsNullOrWhiteSpace(username))
            return BadRequest();
        
        var user = await userManager.FindByNameAsync(username);
        
        if(user == null || user.UserName != username)
            return BadRequest();
        return Ok(await followingService.GetUserFollowersCount(user.Id));
    }

    [Authorize]
    [HttpGet("IsUserFollowing")]
    public async Task<IActionResult> IsUserFollowing(string currentUsername, string targetUsername)
    {
        if (string.IsNullOrWhiteSpace(currentUsername) || string.IsNullOrWhiteSpace(targetUsername))
            return BadRequest();
        var request = new FollowRequestDto(currentUsername, targetUsername);
        return Ok(await followingService.IsUserFollowing(request));
    }

    [Authorize]
    [HttpGet("GetUserFollowers")]
    public async Task<IActionResult> GetUserFollowers(string username)
    {
        if(string.IsNullOrEmpty(username))
            return BadRequest();
        return Ok(await followingService.GetUserFollowers(username));
    }
    [Authorize]
    [HttpGet("GetUserFollowing")]
    public async Task<IActionResult> GetUserFollowing(string username)
    {
        if(string.IsNullOrEmpty(username))
            return BadRequest();
        return Ok(await followingService.GetUserFollowing(username));
    }
    
}
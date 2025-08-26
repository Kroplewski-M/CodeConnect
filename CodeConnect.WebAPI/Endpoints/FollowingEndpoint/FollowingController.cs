using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.ExtensionClasses;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeConnect.WebAPI.Endpoints.FollowingEndpoint;

[Route("api/[controller]")]
[ApiController]
[Authorize(nameof(Consts.TokenType.Access))]
public class FollowingController(IFollowingService followingService, UserManager<ApplicationUser>userManager) : ControllerBase
{
    [HttpPost("FollowUser")]
    public async Task<IActionResult> FollowUser(FollowRequestDto followRequest)
    {
        var userId = User.GetInfo(Consts.ClaimTypes.Id);
        if(string.IsNullOrWhiteSpace(userId))
            return BadRequest(new ServiceResponse(false, "User not found" ));
        
        var response = await followingService.FollowUser(followRequest, userId);
        if(response.Flag)
            return Ok(response);
        return BadRequest(response);
    }
    [HttpPost("UnFollowUser")]
    public async Task<IActionResult> UnFollowUser(FollowRequestDto unFollowRequest)
    {
        if(string.IsNullOrWhiteSpace(unFollowRequest.TargetUsername))
            return BadRequest();
        
        var userId = User.GetInfo(Consts.ClaimTypes.Id);
        if(string.IsNullOrWhiteSpace(userId))
            return BadRequest();
        
        var response = await followingService.UnfollowUser(unFollowRequest, userId);
        if(response.Flag)
            return Ok(response);
        return BadRequest(response);
    }

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

    [HttpGet("IsUserFollowing")]
    public async Task<IActionResult> IsUserFollowing(string targetUsername)
    {
        var userId = User.GetInfo(Consts.ClaimTypes.Id);
        if (string.IsNullOrWhiteSpace(targetUsername) || string.IsNullOrWhiteSpace(userId))
            return BadRequest();
        var request = new FollowRequestDto(targetUsername);
        return Ok(await followingService.IsUserFollowing(request, userId));
    }

    [HttpGet("GetUserFollowers")]
    public async Task<IActionResult> GetUserFollowers(string username, int skip, int take)
    {
        if(string.IsNullOrEmpty(username))
            return BadRequest();
        return Ok(await followingService.GetUserFollowers(username, skip, take));
    }
    [HttpGet("GetUserFollowing")]
    public async Task<IActionResult> GetUserFollowing(string username, int skip, int take)
    {
        if(string.IsNullOrEmpty(username))
            return BadRequest();
        return Ok(await followingService.GetUserFollowing(username, skip, take));
    }
    
}
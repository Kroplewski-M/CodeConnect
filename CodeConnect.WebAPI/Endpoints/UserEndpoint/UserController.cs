using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.ExtensionClasses;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeConnect.WebAPI.Endpoints.UserEndpoint;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService, UserManager<ApplicationUser>userManager,ITokenService tokenService,
    IUserImageService userImageService) : ControllerBase
{
    private TokenResponse GenerateNewToken(ApplicationUser user)
    {
        var claims = user.GetClaimsForUser();
        var token = tokenService.GenerateJwtToken(claims.AsEnumerable(),DateTime.UtcNow.AddMinutes(Consts.Tokens.AuthTokenMins));
        return new TokenResponse(token);
    }
    [Authorize]
    [HttpPost("EditUserDetails")]
    public async Task<IActionResult> EditUserDetails([FromBody] EditProfileForm editProfileForm)
    {
        if (string.IsNullOrWhiteSpace(editProfileForm.Username))
            return BadRequest("No username provided");
        
        var loggedInUser = User.GetInfo(Consts.ClaimTypes.UserName);
        var updateUser = await userManager.FindByNameAsync(editProfileForm.Username);
        
        if (loggedInUser != updateUser?.UserName)
            return Unauthorized("Cannot find user");
        if (updateUser != null)
        {
            var result = await userService.UpdateUserDetails(editProfileForm);
            if(result.Flag)
                return Ok(GenerateNewToken(updateUser));
            return BadRequest(result.Message);
        }
        return Unauthorized("Cannot find user");
    }

    [Authorize]
    [HttpPost("GetUserDetails")]
    public async Task<IActionResult> GetUserDetails([FromBody] string username)
    {
        if(string.IsNullOrWhiteSpace(username))
            return BadRequest("No username provided");
        var userDetails = await userService.GetUserDetails(username);
        if (userDetails == null)
            return NotFound("User does not exist");
        return Ok(userDetails);
    }

    [Authorize]
    [HttpPost("UpdateUserImage")]
    public async Task<IActionResult> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest)
    {
        var loggedInUser = User.GetInfo(Consts.ClaimTypes.UserName);
        if(loggedInUser != updateUserImageRequest.Username)
            return Unauthorized("User not found");
        if(string.IsNullOrWhiteSpace(updateUserImageRequest.ImgBase64) || string.IsNullOrWhiteSpace(updateUserImageRequest.FileName))
            return BadRequest("No image provided");
        if (!string.IsNullOrEmpty(loggedInUser))
        {
            updateUserImageRequest.Username = loggedInUser;
            var response = await userImageService.UpdateUserImage(updateUserImageRequest);
            if (!response.Flag)
                return BadRequest(response.Message);
            //GET NEW TOKEN
            var updatedUser = await userManager.FindByNameAsync(loggedInUser);
            if (updatedUser != null)
            {
                return Ok(GenerateNewToken(updatedUser));
            }
        }
        return Unauthorized("User not found");
    }

    [HttpPost("GetUserInterests")]
    public async Task<IActionResult> GetUserInterests([FromBody]string username)
    {
        if(string.IsNullOrWhiteSpace(username))
            return BadRequest("No username provided");
        var response = await userService.GetUserInterests(username);
        if(response.Flag)
            return Ok(new UserInterestsDto(response.Flag, response.Message, response.Interests));
        return BadRequest(response);
    }

    [Authorize]
    [HttpPut("UpdateUserInterests")]
    public async Task<IActionResult> UpdateUserInterests([FromBody]UpdateTechInterestsDto interests)
    {
        var findUsername = User.GetInfo(Consts.ClaimTypes.UserName);
        if (findUsername != null && findUsername == interests.Username)
        {
            var response = await userService.UpdateUserInterests(interests);
            if(response.Flag)
                return Ok(response);
            return BadRequest(response);
        }
        return Unauthorized(new ServiceResponse(false,"User not found"));
    }

    [HttpGet("GetAllInterests")]
    public async Task<IActionResult> GetAllInterests()
    {
        var interests = await userService.GetAllInterests();
        if (interests.Count == 0)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Service is unavailable. Please try again later.");
        return Ok(interests);   
    }
}
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
[Authorize(nameof(Consts.TokenType.Access))]
public class UserController(IUserService userService,
    IUserImageService userImageService) : ControllerBase
{
    [HttpPost("EditUserDetails")]
    public async Task<IActionResult> EditUserDetails([FromBody] EditProfileForm editProfileForm)
    {
        var loggedInUserId = User.GetInfo(Consts.ClaimTypes.Id);

        if (!string.IsNullOrWhiteSpace(loggedInUserId))
        {
            var result = await userService.UpdateUserDetails(editProfileForm, loggedInUserId);
            if(result.Flag)
                return Ok(result);
            return BadRequest(result.Message);
        }
        return Unauthorized("Cannot find user");
    }

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

    [HttpPost("UpdateUserImage")]
    public async Task<IActionResult> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest)
    {
        var loggedInUser = User.GetInfo(Consts.ClaimTypes.Id);
        if(string.IsNullOrWhiteSpace(updateUserImageRequest.ImgBase64) || string.IsNullOrWhiteSpace(updateUserImageRequest.FileName))
            return BadRequest("No image provided");
        if (!string.IsNullOrEmpty(loggedInUser))
        {
            var response = await userImageService.UpdateUserImage(updateUserImageRequest, loggedInUser);
            if (!response.Flag)
                return BadRequest(response);
            return Ok(response);
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
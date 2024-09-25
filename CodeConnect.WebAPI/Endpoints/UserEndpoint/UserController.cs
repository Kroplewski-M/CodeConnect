using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.DbEnts;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeConnect.WebAPI.Endpoints.UserEndpoint;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService, UserManager<ApplicationUser>userManager,TokenService tokenService,
    IAuthenticateService authenticateService,IUserImageService userImageService) : ControllerBase
{
    private TokenResponse GenerateNewToken(ApplicationUser user)
    {
        var claims = authenticateService.GetClaimsForUser(user);
        return new TokenResponse(tokenService.GenerateJwtToken(claims.AsEnumerable(),DateTime.Now.AddHours(1)));
    }
    [Authorize]
    [HttpPost("EditUserDetails")]
    public async Task<IActionResult> EditUserDetails([FromBody] EditProfileForm editProfileForm)
    {
        var username = User.FindFirst(Constants.ClaimTypes.UserName)?.Value;
        if (username != editProfileForm.Username)
            return Unauthorized();
        await userService.UpdateUserDetails(editProfileForm);
        var updatedUser = await userManager.FindByNameAsync(editProfileForm.Username);
        if (updatedUser != null)
        {
            return Ok(GenerateNewToken(updatedUser));
        }
        return Unauthorized();
    }

    [Authorize]
    [HttpPost("GetUserDetails")]
    public async Task<IActionResult> GetUserDetails([FromBody] string username)
    {
        var user = await userService.GetUserDetails(username);
        if (user == null)
        {
            return NotFound("User does not exist");
        }
        return Ok(user);
    }

    [Authorize]
    [HttpPost("UpdateUserImage")]
    public async Task<IActionResult> UpdateUserImage([FromForm]IFormFile image, [FromForm] int typeOfImage)
    {
        var username = User.FindFirst(Constants.ClaimTypes.UserName)?.Value;
        if (!string.IsNullOrEmpty(username))
        {
            var updateUserImageRequest = new UpdateUserImageRequest
            {
                ImageStream = image.OpenReadStream(),
                ContentType = image.ContentType,
                FileName = image.FileName,
                TypeOfImage = (Constants.ImageTypeOfUpdate)typeOfImage,
                Username = username
            };

            var response = await userImageService.UpdateUserImage(updateUserImageRequest);
            if (!response.Flag)
                return BadRequest(response.Message);
            //GET NEW TOKEN
            var updatedUser = await userManager.FindByNameAsync(username);
            if (updatedUser != null)
            {
                return Ok(GenerateNewToken(updatedUser));
            }
        }
        return BadRequest("User not found or image is null");
    }

    [HttpPost("GetUserInterests")]
    public async Task<IActionResult> GetUserInterests([FromBody]string username)
    {
        var response = await userService.GetUserInterests(username);
        if(response.flag)
            return Ok(new UserInterestsDto(response.flag, response.message, response.Interests));
        return BadRequest(response);
    }

    [Authorize]
    [HttpPost("UpdateUserInterests")]
    public async Task<IActionResult> UpdateUserInterests(List<UserInterestsDto> interests)
    {
        var username = User.FindFirst(Constants.ClaimTypes.UserName)?.Value;
        if (username != null)
        {
            var response = await userService.UpdateUserInterests(username, interests);
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
        if (!interests.Any())
            return BadRequest("Failed to fetch interests");
        return Ok(await userService.GetAllInterests());   
    }
}
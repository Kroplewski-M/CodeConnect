using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
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
            var claims = authenticateService.GetClaimsForUser(updatedUser);
            return Ok(new TokenResponse(tokenService.GenerateJwtToken(claims.AsEnumerable(),DateTime.Now.AddHours(1))));
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

            var url = await userImageService.UpdateUserImage(updateUserImageRequest);
            if (string.IsNullOrEmpty(url))
                return BadRequest("An error occurred");

            return Ok(url);
        }
        return BadRequest("User not found or image is null");

    }
}
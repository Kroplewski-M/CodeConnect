using DomainLayer.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CodeConnect.WebAPI.Endpoints.UserEndpoint;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    [HttpPost("EditUserDetails")]
    public async Task<IActionResult> EditUserDetails([FromBody] EditProfileForm editProfileForm)
    {
        return Ok(editProfileForm);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserID(string username)
    {
        return Ok(username);
    }
}
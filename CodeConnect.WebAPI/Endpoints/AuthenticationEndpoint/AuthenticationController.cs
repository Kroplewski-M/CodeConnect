using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CodeConnect.WebAPI.Endpoints.AuthenticationEndpoint;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(IAuthenticateService authenticateService) : ControllerBase
{
    [HttpPost("CreateUser")]
    public async Task<IActionResult> CreateUser([FromBody]RegisterFormViewModel registerForm)
    {
        var result = await authenticateService.CreateUser(registerForm);
        if (result.Token != "")
        {
            return Ok(result);
        }
        return BadRequest("Account creation Failed");
    }
}
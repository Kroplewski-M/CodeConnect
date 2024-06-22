using System.Security.Claims;
using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CodeConnect.WebAPI.Endpoints.AuthenticationEndpoint;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(IAuthenticateService authenticateService, 
    TokenService tokenService) : ControllerBase
{
    [HttpPost("CreateUser")]
    public async Task<IActionResult> CreateUser([FromBody]RegisterFormViewModel registerForm)
    {
        var result = await authenticateService.CreateUser(registerForm);
        if (result.Flag)
            return Ok(result);
        return BadRequest("Account creation Failed");
    }

    [HttpPost("LoginUser")]
    public async Task<IActionResult> LoginUser([FromBody] LoginFormViewModel loginForm)
    {
        var result = await authenticateService.LoginUser(loginForm);
        if (result.Flag)
            return Ok(result);
        return BadRequest("Account Not Found");
    }
    [HttpPost("ValidateToken")]
    public IActionResult ValidateToken([FromBody]string token)
    {
        var result = tokenService.ValidateToken(token);
        if (result.Flag)
            return Ok(result);
        return Unauthorized();
    }
    [HttpPost("RefreshToken")]
    public IActionResult RefreshToken([FromBody]string token)
    {
        return Ok(tokenService.RefreshToken(token));
    }
}
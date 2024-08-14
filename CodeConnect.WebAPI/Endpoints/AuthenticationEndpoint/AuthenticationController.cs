using System.Security.Claims;
using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeConnect.WebAPI.Endpoints.AuthenticationEndpoint;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(IAuthenticateService authenticateService, 
    TokenService tokenService) : ControllerBase
{
    [HttpPost("RegisterUser")]
    public async Task<IActionResult> RegisterUser([FromBody]RegisterForm registerForm)
    {
        var result = await authenticateService.CreateUser(registerForm);
        if (result.Flag)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("LoginUser")]
    public async Task<IActionResult> LoginUser([FromBody] LoginForm loginForm)
    {
        var result = await authenticateService.LoginUser(loginForm);
        if (result.Flag)
            return Ok(result);
        return BadRequest(result);
    }
    [HttpPost("ValidateToken")]
    public IActionResult ValidateToken([FromBody]string token)
    {
        var result = tokenService.ValidateToken(token);
        if (result.Flag)
            return Ok(result);
        return Unauthorized(result);
    }
    [HttpGet("RefreshToken")]
    public IActionResult RefreshToken()
    {
        var token = tokenService.GenerateJwtToken(User.Claims.ToList(), DateTime.UtcNow.AddMinutes(60));
        return Ok(tokenService.GenerateJwtToken(User.Claims.ToList(),DateTime.UtcNow.AddMinutes(60)));
    }
}
using System.Reflection.Metadata;
using System.Security.Claims;
using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace CodeConnect.WebAPI.Endpoints.AuthenticationEndpoint;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(IAuthenticateService authenticateService, 
    ITokenService tokenService) : ControllerBase
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
    [Authorize]
    [HttpGet("RefreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();
        if(string.IsNullOrWhiteSpace(authorizationHeader))
            return Unauthorized("No refresh token");
        var token = "";
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
        {
            token = authorizationHeader.Substring("Bearer ".Length).Trim();
        }

        var name = User?.Identity?.Name;
        if(name == null)
            return Unauthorized("Could not retrieve user");
        var response = await tokenService.RefreshUserTokens(name,token);
        if(response.Flag)
            return Ok(response);
        return BadRequest(response);

    }
}
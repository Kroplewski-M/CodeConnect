using System.Reflection.Metadata;
using System.Security.Claims;
using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Constants = DomainLayer.Constants.Constants;

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
    [Authorize]
    [HttpGet("RefreshToken")]
    public IActionResult RefreshToken()
    {
        var token = tokenService.GenerateJwtToken(User.Claims.ToList(), DateTime.UtcNow.AddMinutes(Constants.Tokens.AuthTokenMins));
        var refreshToken = tokenService.GenerateJwtToken(User.Claims.ToList(), DateTime.UtcNow.AddMinutes(Constants.Tokens.RefreshTokenMins));
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(refreshToken))
        {
            return Ok(new AuthResponse(true, token, refreshToken, "success"));
        }
        return BadRequest(new AuthResponse(false,"","","Error occured"));
    }
}
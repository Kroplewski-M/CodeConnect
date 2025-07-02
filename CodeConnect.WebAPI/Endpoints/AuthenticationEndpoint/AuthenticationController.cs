using System.Reflection.Metadata;
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
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
    public async Task<IActionResult> RegisterUser([FromBody]RegisterForm registerForm, [FromHeader(Name = Consts.Headers.DeviceId)]string deviceId)
    {
        var result = await authenticateService.CreateUser(registerForm, deviceId);
        if (result.Flag)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("LoginUser")]
    public async Task<IActionResult> LoginUser([FromBody] LoginForm loginForm, [FromHeader(Name = Consts.Headers.DeviceId)]string deviceId)
    {
        var result = await authenticateService.LoginUser(loginForm, deviceId);
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
    [Authorize(nameof(Consts.TokenType.Refresh))]
    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken([FromHeader(Name = Consts.Headers.DeviceId)]string deviceId)
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();
        if(string.IsNullOrWhiteSpace(authorizationHeader))
            return Unauthorized(new AuthResponse(false,"","","error refreshing token"));
        var token = "";
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
        {
            token = authorizationHeader.Substring("Bearer ".Length).Trim();
        }
        var response = await tokenService.RefreshUserTokens(token, deviceId);
        if(response.Flag)
            return Ok(response);
        return BadRequest(response);
    }
}
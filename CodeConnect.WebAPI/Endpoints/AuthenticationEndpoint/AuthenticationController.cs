using System.Reflection.Metadata;
using System.Security.Claims;
using ApplicationLayer.APIServices;
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
    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();
        if(string.IsNullOrWhiteSpace(authorizationHeader))
            return Unauthorized(new AuthResponse(false,"","","error refreshing token"));
        var token = "";
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
        {
            token = authorizationHeader.Substring("Bearer ".Length).Trim();
        }
        var res = tokenService.ValidateToken(token);
        var name = res?.ClaimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == Consts.ClaimTypes.UserName);
        if(name == null)
            return Unauthorized(new AuthResponse(false,"","","error refreshing token"));
        var response = await tokenService.RefreshUserTokens(name.Value,token);
        if(response.Flag)
            return Ok(response);
        return BadRequest(response);
    }
}
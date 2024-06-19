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
        if (result.flag)
        {
            return Ok(result);
        }
        return BadRequest("Account creation Failed");
    }
    [HttpPost("ValidateToken")]
    public ClaimsPrincipal? ValidateToken([FromBody]string token)
    {
        return tokenService.ValidateToken(token);
    }
    [HttpPost("RefreshToken")]
    public TokenResponse RefreshToken([FromBody]string token)
    {
        return tokenService.RefreshToken(token);
    }
}
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
    ITokenService tokenService, IConfiguration config) : ControllerBase
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
    [HttpGet("GithubLogin")]
    public IActionResult GithubLogin()
    {
        var clientId = config["GithubAuth:ClientId"];
        var state = Guid.NewGuid().ToString(); 
        var githubUrl = $"https://github.com/login/oauth/authorize" +
                        $"?client_id={clientId}&scope=read:user%20user:email&state={state}";

        return Ok(githubUrl);
    }

    [HttpGet("github/callback")]
    public async Task<IActionResult> GitHubCallback(string code, string state)
    {
        var clientId = config["GithubAuth:ClientId"];
        var clientSecret = config["GithubAuth:Secret"];

        using var httpClient = new HttpClient();

        var tokenResponse = await httpClient.PostAsync("https://github.com/login/oauth/access_token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId ?? "" },
                { "client_secret", clientSecret ?? "" },
                { "code", code }
            }));
        var responseContent = await tokenResponse.Content.ReadAsStringAsync();
        var query = System.Web.HttpUtility.ParseQueryString(responseContent);
        var accessToken = query["access_token"];

        // Get user info
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CodeConnect");
        var userResponse = await httpClient.GetAsync("https://api.github.com/user");
        var emails = await httpClient.GetAsync("https://api.github.com/user/emails");
        userResponse.EnsureSuccessStatusCode();

        var userJson = await userResponse.Content.ReadAsStringAsync();
        var emailsJson = await emails.Content.ReadAsStringAsync();
        
        return Ok();
    }

    [Authorize]
    [HttpGet("RefreshToken")]
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
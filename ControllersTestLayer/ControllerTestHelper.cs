using System.Security.Claims;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestLayer;

public static class ControllerTestHelper
{
    public static void SetMockUserInContext(ControllerBase controller, string username, string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(Consts.ClaimTypes.UserName, username),
            new Claim(Consts.ClaimTypes.Id, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }   
}
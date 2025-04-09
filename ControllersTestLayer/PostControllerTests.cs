using System.Security.Claims;
using ApplicationLayer.Interfaces;
using CodeConnect.WebAPI.Endpoints.PostEndpoint;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace TestLayer;

public class PostControllerTests
{
    private readonly Mock<IPostService> _mockPostService;
    private readonly PostController _postController;
    
    public PostControllerTests()
    {
        _mockPostService = new Mock<IPostService>();
        _postController = new PostController(_mockPostService.Object);
        
        var currentUsername = "testUsername";
        var claims = new[] { new Claim(Consts.ClaimTypes.UserName, currentUsername) };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _postController.ControllerContext = new ControllerContext(){ HttpContext = httpContext };
    }
}
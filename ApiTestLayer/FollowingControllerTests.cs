using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using CodeConnect.WebAPI.Endpoints.FollowingEndpoint;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace TestLayer;

public class FollowingControllerTests
{
    private readonly Mock<IAuthenticateService> _authenticateServiceMock;
    private readonly Mock<IFollowingService> _followServiceMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly FollowingController _followingController;
    public FollowingControllerTests()
    {
        _authenticateServiceMock = new Mock<IAuthenticateService>();
        _followServiceMock = new Mock<IFollowingService>();
        
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, 
            null!, null!, null!, null!, null!, null!, null!, null!
        );
        _followingController = new FollowingController(_followServiceMock.Object, _userManagerMock.Object);
        
        var currentUsername = "testUsername";
        var claims = new[] { new Claim(Consts.ClaimTypes.UserName, currentUsername) };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _followingController.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }
    [Fact]
    public async Task FollowUser_InvalidDto_ShouldReturnUnauthorized()
    {
        //Arrange
        var request = new FollowRequestDto("wrongUsername","testUsername2");
        //Act
        var result = await _followingController.FollowUser(request);
        var unauthorizedResult = result as UnauthorizedResult;
        
        // Assert
        Assert.NotNull(unauthorizedResult);
        Assert.Equal((int)System.Net.HttpStatusCode.Unauthorized, unauthorizedResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }

    [Fact]
    public async Task FollowUser_ValidDto_ButCantFindUser_ShouldReturnUnauthorized()
    {
        //Arrange
        var request = new FollowRequestDto("testUsername","testUsername2");
        var currentUsername = "";
        var claims = new[] { new Claim(Consts.ClaimTypes.UserName, currentUsername) };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _followingController.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        //Act
        var result = await _followingController.FollowUser(request);
        var unauthorizedResult = result as UnauthorizedResult;
        
        //Assert
        Assert.NotNull(unauthorizedResult);
        Assert.Equal((int)System.Net.HttpStatusCode.Unauthorized, unauthorizedResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }
}
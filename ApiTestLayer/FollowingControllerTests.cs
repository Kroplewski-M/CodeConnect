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
    public async Task FollowUser_InvalidDto_ShouldReturnBadRequest()
    {
        //Arrange
        var request = new FollowRequestDto("wrongUsername","testUsername2");
        //Act
        var result = await _followingController.FollowUser(request);
        var badRequestResult = result as BadRequestResult;
        
        // Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }

    [Fact]
    public async Task FollowUser_ValidDto_ButCantFindUser_ShouldReturnBadRequest()
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
        var badRequestResult = result as BadRequestResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }

    [Fact]
    public async Task FollowUser_ValidDto_ValidUser_ShouldReturnOk()
    {
        //Arrange 
        var request = new FollowRequestDto("testUsername","testUsername2");
        var expectedResponse = new ServiceResponse( true,  "User followed successfully");
        _followServiceMock.Setup(x=> x.FollowUser(request)).ReturnsAsync(expectedResponse);
        
        //Act
        var result = await _followingController.FollowUser(request);
        var okResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)System.Net.HttpStatusCode.OK, okResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(request), Times.Once);
    }

    [Fact]
    public async Task UnFollowUser_EmptyCurrentUsername_ShouldReturnBadRequest()
    {
        //Arrange
        var request = new FollowRequestDto("","testUsername2");
        
        //Act
        var result = await _followingController.UnFollowUser(request);
        var badRequestResult = result as BadRequestResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }
    [Fact]
    public async Task UnFollowUser_EmptyTargetUsername_ShouldReturnBadRequest()
    {
        //Arrange
        var request = new FollowRequestDto("testUsername","");
        
        //Act
        var result = await _followingController.UnFollowUser(request);
        var badRequestResult = result as BadRequestResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }
    [Fact]
    public async Task UnFollowUser_EmptyBothUsernames_ShouldReturnBadRequest()
    {
        //Arrange
        var request = new FollowRequestDto("","");
        
        //Act
        var result = await _followingController.UnFollowUser(request);
        var badRequestResult = result as BadRequestResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }

    [Fact]
    public async Task UnfollowUser_ValidDto_WrongCurrentUser_ShouldReturnBadRequest()
    {
        //Arrange
        var request = new FollowRequestDto("wrongUsername","testUsername2");
        
        //Act
        var result = await _followingController.UnFollowUser(request);
        var badRequestResult = result as BadRequestResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }

    [Fact]
    public async Task UnfollowUser_ValidDto_ValidUser_ShouldReturnOk()
    {
        //Arrange
        var request = new FollowRequestDto("testUsername","testUsername2");
        var expectedResult = new ServiceResponse( true,  "User unfollowed successfully");
        _followServiceMock.Setup(x=> x.UnfollowUser(request)).ReturnsAsync(expectedResult);
        //Act
        var result = await _followingController.UnFollowUser(request);
        var okResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)System.Net.HttpStatusCode.OK, okResult.StatusCode);
        _followServiceMock.Verify(x => x.UnfollowUser(It.IsAny<FollowRequestDto>()), Times.Once);
    }

    [Fact]
    public async Task UserFollowersCount_EmptyUsername_ShouldReturnBadRequest()
    {
        //Arrange
        var request = "";
        
        //Act
        var result = await _followingController.UserFollowersCount(request);
        var badRequestResult = result as BadRequestResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }

    [Fact]
    public async Task UserFollowersCount_CantFindUser_ShouldReturnBadRequest()
    {
        //Arrange
        var request = "testUsername";
        _userManagerMock.Setup(x => x.FindByNameAsync(request)).ReturnsAsync(new ApplicationUser());
        
        //Act
        var result = await _followingController.UserFollowersCount(request);
        var badRequestResult = result as BadRequestResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }

    [Fact]
    public async Task UserFollowersCount_ValidUser_ShouldReturnOk()
    {
        //Arrange
        var request = "testUsername";
        _userManagerMock.Setup(x => x.FindByNameAsync(request)).ReturnsAsync(new ApplicationUser(){UserName = "testUsername"});
        
        //Act
        var result = await _followingController.UserFollowersCount(request);
        var okResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)System.Net.HttpStatusCode.OK, okResult.StatusCode);
        _followServiceMock.Verify(x => x.GetUserFollowersCount(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task IsUserFollowing_EmptyCurrentUsername_ShouldReturnBadRequest()
    {
        //Arrange
        var currentUsername = "";
        var targetUsername = "testUsername2";
        
        //Act
        var result = await _followingController.IsUserFollowing(currentUsername, targetUsername);
        var badRequestResult = result as BadRequestResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }
    [Fact]
    public async Task IsUserFollowing_EmptyTargetUsername_ShouldReturnBadRequest()
    {
        //Arrange
        var currentUsername = "testUsername";
        var targetUsername = "";
        
        //Act
        var result = await _followingController.IsUserFollowing(currentUsername, targetUsername);
        var badRequestResult = result as BadRequestResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }
    [Fact]
    public async Task IsUserFollowing_EmptyUsernames_ShouldReturnBadRequest()
    {
        //Arrange
        var currentUsername = "";
        var targetUsername = "";
        
        //Act
        var result = await _followingController.IsUserFollowing(currentUsername, targetUsername);
        var badRequestResult = result as BadRequestResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _followServiceMock.Verify(x => x.FollowUser(It.IsAny<FollowRequestDto>()), Times.Never);
    }
    [Fact]
    public async Task IsUserFollowing_CorrectUsernames_ShouldReturnOk()
    {
        //Arrange
        var currentUsername = "testUsername";
        var targetUsername = "testUsername2";
        var request = new FollowRequestDto(currentUsername, targetUsername);
        _followServiceMock.Setup(x => x.IsUserFollowing(request)).ReturnsAsync(true);
        
        //Act
        var result = await _followingController.IsUserFollowing(currentUsername, targetUsername);
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal((int)System.Net.HttpStatusCode.OK, okRequestResult.StatusCode);
        _followServiceMock.Verify(x => x.IsUserFollowing(It.IsAny<FollowRequestDto>()), Times.Once);
    }
}
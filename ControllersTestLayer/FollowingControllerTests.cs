using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
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
    private readonly Mock<IFollowingService> _followingServiceMock;
    private readonly FollowingController _followingController;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    public FollowingControllerTests()
    {
        _followingServiceMock = new Mock<IFollowingService>();
        // Properly mock UserManager<ApplicationUser>
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null!, // OptionsAccessor
            null!, // PasswordHasher
            null!, // UserValidators
            null!, // PasswordValidators
            null!, // KeyNormalizer
            null!, // Errors
            null!, // TokenProviders
            null!  // Logger
        );
        _followingController = new FollowingController(_followingServiceMock.Object,_userManagerMock.Object);
        ControllerTestHelper.SetMockUserInContext(_followingController, "default-user", "test-user-id");
    }
    [Fact]
    public async Task FollowUser_ValidRequest_ShouldReturnOkResponse()
    {
        // Arrange
        var request = new FollowRequestDto( "TargetUser");
        var serviceResponse = new ServiceResponse(true, "Follow successful");

        _followingServiceMock
            .Setup(service => service.FollowUser(request, It.IsAny<string>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _followingController.FollowUser(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(serviceResponse, okResult.Value);
        _followingServiceMock.Verify(service => service.FollowUser(request, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task FollowUser_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new FollowRequestDto( "TargetUser");
        ControllerTestHelper.SetMockUserInContext(_followingController, "", "");
        // Act
        var result = await _followingController.FollowUser(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var s = Assert.IsType<ServiceResponse>(badRequestResult.Value);
        Assert.False(s.Flag);
        _followingServiceMock.Verify(service => service.FollowUser(request, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UnfollowUser_ValidRequest_ShouldReturnOkResponse()
    {
        // Arrange
        var request = new FollowRequestDto( "TargetUser");
        var serviceResponse = new ServiceResponse(true, "Unfollow successful");

        _followingServiceMock
            .Setup(service => service.UnfollowUser(request, It.IsAny<string>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _followingController.UnFollowUser(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(serviceResponse, okResult.Value);
        _followingServiceMock.Verify(service => service.UnfollowUser(request, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UnfollowUser_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new FollowRequestDto( "TargetUser");
        var serviceResponse = new ServiceResponse(false, "Invalid request");

        _followingServiceMock
            .Setup(service => service.UnfollowUser(request, It.IsAny<string>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _followingController.UnFollowUser(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(serviceResponse, badRequestResult.Value);
        _followingServiceMock.Verify(service => service.UnfollowUser(request, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetUserFollowers_ValidRequest_ShouldReturnFollowersList()
    {
        // Arrange
        var username = "SomeUsername";
        var followersList = new List<UserBasicDto>
        {
            new UserBasicDto("","User1","",""),
            new UserBasicDto("","User2", "","")
        };

        _followingServiceMock
            .Setup(service => service.GetUserFollowers(username, 0, 10))
            .ReturnsAsync(followersList);

        // Act
        var result = await _followingController.GetUserFollowers(username, 0, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(followersList, okResult.Value);
        _followingServiceMock.Verify(service => service.GetUserFollowers(username, 0, 10), Times.Once);
    }

    [Fact]
    public async Task GetUserFollowers_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var username = "";

        // Act
        var result = await _followingController.GetUserFollowers(username, 0, 10);

        // Assert
        Assert.IsType<BadRequestResult>(result);
        _followingServiceMock.Verify(service => service.GetUserFollowers(It.IsAny<string>(), 0, 10), Times.Never);
    }

    [Fact]
    public async Task GetUserFollowing_ValidRequest_ShouldReturnFollowingList()
    {
        // Arrange
        var username = "SomeUsername";
        var followingList = new List<UserBasicDto> { new UserBasicDto("","User1", "",""), new UserBasicDto("","User2", "","") };

        _followingServiceMock
            .Setup(service => service.GetUserFollowing(username, 0, 10))
            .ReturnsAsync(followingList);

        // Act
        var result = await _followingController.GetUserFollowing(username, 0, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(followingList, okResult.Value);
        _followingServiceMock.Verify(service => service.GetUserFollowing(username, 0, 10), Times.Once);
    }

    [Fact]
    public async Task GetUserFollowing_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var username = "";

        // Act
        var result = await _followingController.GetUserFollowing(username, 0, 10);

        // Assert
        Assert.IsType<BadRequestResult>(result);
        _followingServiceMock.Verify(service => service.GetUserFollowing(It.IsAny<string>(), 0, 10), Times.Never);
    }

    [Fact]
    public async Task IsUserFollowing_ValidRequest_ShouldReturnTrue()
    {
        // Arrange
        var request = new FollowRequestDto( "Bob");

        _followingServiceMock
            .Setup(service => service.IsUserFollowing(request, It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x=> x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser(){UserName = "testUsername"});
        _userManagerMock.Setup(x=> x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser(){UserName = "testUsername2"});
        // Act
        var result = await _followingController.IsUserFollowing("bob");
      
        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task IsUserFollowing_InvalidRequest_ShouldReturnBadRequest()
    {
        ControllerTestHelper.SetMockUserInContext(_followingController, "", "");
        // Act
        var result = await _followingController.IsUserFollowing("Bob");

        // Assert
        Assert.IsType<BadRequestResult>(result);
        _followingServiceMock.Verify(service => service.IsUserFollowing(It.IsAny<FollowRequestDto>(), It.IsAny<string>()), Times.Never);
    }
}
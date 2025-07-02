using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.User;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApiApplicationLayer;

namespace ApiServiceTests;

[Collection("DatabaseCollection")]
public class FollowingServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly FollowingService _followingService;

    public FollowingServiceTests(DatabaseFixture databaseFixture)
    {       
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!,
            null!, null!, null!);
        _context = databaseFixture.Context;
        _followingService = new FollowingService(_userManager.Object, _context);
    }

    [Fact]
    public async Task GetUserFollowersCount_ShouldReturnCorrectCounts()
    {
        // Arrange
        var userId = "user-5";
        var user = new ApplicationUser { Id = userId };

        _context.FollowUsers.AddRange(
            new Followers { FollowerUserId = userId, FollowedUserId = "user-6" },
            new Followers { FollowerUserId = "user-7", FollowedUserId = userId },
            new Followers { FollowerUserId = "user-8", FollowedUserId = userId }
        );
        await _context.SaveChangesAsync();

        _userManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _followingService.GetUserFollowersCount(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.FollowersCount);
        Assert.Equal(1, result.FollowingCount);
        _context.ChangeTracker.Clear();
    }

    [Theory]
    [InlineData( "SomeUsername", false, 0)] // Current username is empty
    [InlineData( "", false, 0)] // Target username is empty
    public async Task FollowUser_InvalidInput_ShouldReturnBadServiceResponse(string targetUsername, bool expectedResult, int userManagerCalls)
    {
        // Arrange
        var request = new FollowRequestDto(targetUsername);

        // Act 
        var result = await _followingService.FollowUser(request);

        // Assert 
        Assert.NotNull(result);
        Assert.Equal(expectedResult, result.Flag);
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Exactly(userManagerCalls));
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task FollowUser_CannotFindUser_ShouldReturnBadServiceResponse()
    {
        // Arrange
        var request = new FollowRequestDto("SomeOtherUsername");
        _userManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        // Act
        var result = await _followingService.FollowUser(request, "id");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task FollowUser_ValidData_UserExists_ShouldReturnSuccess()
    {
        // Arrange
        var currentUser = new ApplicationUser { UserName = "Alice", Id = "3" };
        var targetUser = new ApplicationUser { UserName = "Bob", Id = "4" };
        var request = new FollowRequestDto(targetUser.UserName);

        _userManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(currentUser);
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(targetUser);

        // Act
        var result = await _followingService.FollowUser(request, "id");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Flag);

        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
        var followEntry = await _context.FollowUsers.FirstOrDefaultAsync(f => f.FollowerUserId == "3" && f.FollowedUserId == "4");
        Assert.NotNull(followEntry);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task FollowUser_UserAlreadyFollowed_ShouldReturnBadServiceResponse()
    {
        // Arrange
        var currentUser = new ApplicationUser { UserName = "Alice", Id = "1" };
        var targetUser = new ApplicationUser { UserName = "Bob", Id = "2" };

        _context.FollowUsers.Add(new Followers { Id = 1, FollowerUserId = "1", FollowedUserId = "2" });
        await _context.SaveChangesAsync();

        var request = new FollowRequestDto(targetUser.UserName);
        _userManager.Setup(x => x.FindByIdAsync(currentUser.UserName)).ReturnsAsync(currentUser);
        _userManager.Setup(x => x.FindByNameAsync(targetUser.UserName)).ReturnsAsync(targetUser);

        // Act
        var result = await _followingService.FollowUser(request, "id");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(1));
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Exactly(1));
        _context.ChangeTracker.Clear();
    }

    [Theory]
    [InlineData( "SomeUsername", false, 0)] // Current username is empty
    [InlineData("", false, 0)] // Target username is empty
    public async Task UnfollowUser_InvalidInput_ShouldReturnBadServiceResponse(string targetUsername, bool expectedResult, int userManagerCalls)
    {
        // Arrange
        var request = new FollowRequestDto(targetUsername);

        // Act 
        var result = await _followingService.UnfollowUser(request);

        // Assert 
        Assert.NotNull(result);
        Assert.Equal(expectedResult, result.Flag);
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Exactly(userManagerCalls));
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task UnfollowUser_UserNotFound_ShouldReturnBadServiceResponse()
    {
        // Arrange
        var request = new FollowRequestDto( "SomeOtherUsername");
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        _userManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _followingService.UnfollowUser(request, "id");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(1));
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Exactly(1));
        
        _context.ChangeTracker.Clear();
    }
}
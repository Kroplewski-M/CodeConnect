using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.User;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

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
    public async Task GetUserFollowersCount_ReturnsCorrectCounts()
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
        Assert.Equal(2, result.FollowersCount);
        Assert.Equal(1, result.FollowingCount);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task FollowUser_EmptyCurrentUsername_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var request = new FollowRequestDto("", "SomeUsername");

        //Act 
        var result = await _followingService.FollowUser(request);

        //Assert 
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task FollowUser_EmptyTargetUsername_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var request = new FollowRequestDto("SomeUsername", "");

        //Act 
        var result = await _followingService.FollowUser(request);

        //Assert 
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task FollowUser_CannotFindUser_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var request = new FollowRequestDto("SomeUsername", "SomeOtherUsername");
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);

        //Act
        var result = await _followingService.FollowUser(request);

        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task FollowUser_GoodData_UserExists_ShouldReturnOk()
    {
        //Arrange
        var currentUser = new ApplicationUser { UserName = "Alice", Id = "3" };
        var targetUser = new ApplicationUser { UserName = "Bob", Id = "4" };
        var request = new FollowRequestDto(currentUser.UserName, targetUser.UserName);
        _userManager.Setup(x => x.FindByNameAsync("Alice")).ReturnsAsync(currentUser);
        _userManager.Setup(x => x.FindByNameAsync("Bob")).ReturnsAsync(targetUser);
        //Act 
        var result = await _followingService.FollowUser(request);

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Flag);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
        var followEntry = await _context.FollowUsers
            .FirstOrDefaultAsync(f => f.FollowerUserId == "3" && f.FollowedUserId == "4");
        Assert.NotNull(followEntry);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task FollowUser_GoodData_UserAlreadyFollowed_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var currentUser = new ApplicationUser { UserName = "Alice", Id = "1" };
        var targetUser = new ApplicationUser { UserName = "Bob", Id = "2" };

        _context.FollowUsers.Add(new Followers() { Id = 1, FollowerUserId = "1", FollowedUserId = "2" });
        await _context.SaveChangesAsync();

        var request = new FollowRequestDto(currentUser.UserName, targetUser.UserName);
        _userManager.Setup(x => x.FindByNameAsync("Alice")).ReturnsAsync(currentUser);
        _userManager.Setup(x => x.FindByNameAsync("Bob")).ReturnsAsync(targetUser);
        //Act 
        var result = await _followingService.FollowUser(request);

        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
        var followEntry = await _context.FollowUsers
            .FirstOrDefaultAsync(f => f.FollowerUserId == "1" && f.FollowedUserId == "2");
        Assert.NotNull(followEntry);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task UnFollowUser_EmptyCurrentUsername_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var request = new FollowRequestDto("", "SomeUsername");
        //Act 
        var result = await _followingService.UnfollowUser(request);

        //Assert 
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task UnFollowUser_EmptyTargetUsername_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var request = new FollowRequestDto("SomeUsername", "");

        //Act 
        var result = await _followingService.UnfollowUser(request);

        //Assert 
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task UnFollowUser_CannotFindUser_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var request = new FollowRequestDto("SomeUsername", "SomeOtherUsername");
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);

        //Act
        var result = await _followingService.UnfollowUser(request);

        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task UnFollowUser_GoodData_UserExists_ShouldReturnOk()
    {
        //Arrange
        var currentUser = new ApplicationUser { UserName = "Alice", Id = "17" };
        var targetUser = new ApplicationUser { UserName = "Bob", Id = "22" };

        _context.FollowUsers.Add(new Followers() { Id = 12, FollowerUserId = "17", FollowedUserId = "22" });
        await _context.SaveChangesAsync();
        var request = new FollowRequestDto(currentUser.UserName, targetUser.UserName);
        _userManager.Setup(x => x.FindByNameAsync("Alice")).ReturnsAsync(currentUser);
        _userManager.Setup(x => x.FindByNameAsync("Bob")).ReturnsAsync(targetUser);
        //Act 
        var result = await _followingService.UnfollowUser(request);

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Flag);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
        var followEntry = await _context.FollowUsers
            .FirstOrDefaultAsync(f => f.FollowerUserId == "17" && f.FollowedUserId == "22");
        Assert.Null(followEntry);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task IsUserFollowing_NoCurrentUser_ShouldReturnFalse()
    {
        //Arrange
        var request = new FollowRequestDto("", "SomeOtherUsername");

        //Act
        var result = await _followingService.IsUserFollowing(request);

        //Assert
        Assert.False(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _context.ChangeTracker.Clear();
    }
    [Fact]
    public async Task IsUserFollowing_NoTargetUser_ShouldReturnFalse()
    {
        //Arrange
        var request = new FollowRequestDto("SomeUsername", "");
        //Act
        var result = await _followingService.IsUserFollowing(request);

        //Assert
        Assert.False(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task IsUserFollowing_UserDoesntExist_ShouldReturnFalse()
    {
        //Arrange
        var request = new FollowRequestDto("SomeUsername", "SomeOtherUsername");

        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);
        //Act
        var result = await _followingService.IsUserFollowing(request);

        //Assert
        Assert.False(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task IsUserFollowing_UserExists_IsNotFollowingUser_ShouldReturnFalse()
    {
        //Arrange
        var request = new FollowRequestDto("SomeUsername", "SomeOtherUsername");
        var currentUser = new ApplicationUser { UserName = "Alice", Id = "1" };
        var targetUser = new ApplicationUser { UserName = "Bob", Id = "2" };
        _userManager.Setup(x => x.FindByNameAsync("Alice")).ReturnsAsync(currentUser);
        _userManager.Setup(x => x.FindByNameAsync("Bob")).ReturnsAsync(targetUser);
        //Act
        var result = await _followingService.IsUserFollowing(request);

        //Assert
        Assert.False(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
        _context.ChangeTracker.Clear();
    }
    [Fact]
    public async Task IsUserFollowing_UserExists_IsFollowingUser_ShouldReturnFalse()
    {
        //Arrange
        var request = new FollowRequestDto("Alice", "Bob");
        var currentUser = new ApplicationUser { UserName = "Alice", Id = "19" };
        var targetUser = new ApplicationUser { UserName = "Bob", Id = "20" };
        _context.FollowUsers.Add(new Followers() { Id = 4, FollowerUserId = "19", FollowedUserId = "20" });
        await _context.SaveChangesAsync();
        _userManager.Setup(x => x.FindByNameAsync("Alice")).ReturnsAsync(currentUser);
        _userManager.Setup(x => x.FindByNameAsync("Bob")).ReturnsAsync(targetUser);
        //Act
        var result = await _followingService.IsUserFollowing(request);

        //Assert
        Assert.True(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task GetUserFollowers_EmptyUser_ShouldReturnEmptyList()
    {
         //Arrange 
         var username = "";
         //Act
         var result = await _followingService.GetUserFollowers(username);
         //Assert
         Assert.Empty(result);
         _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
         _context.ChangeTracker.Clear();
    }
    [Fact]
    public async Task GetUserFollowers_CannotFindUser_ShouldReturnEmptyList()
    {
        //Arrange 
        var username = "SomeUsername";
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);
        //Act
        var result = await _followingService.GetUserFollowers(username);
        //Assert
        Assert.Empty(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
        _context.ChangeTracker.Clear();
    }
    [Fact]
    public async Task GetUserFollowing_EmptyUser_ShouldReturnEmptyList()
    {
         //Arrange 
         var username = "";
         //Act
         var result = await _followingService.GetUserFollowing(username);
         //Assert
         Assert.Empty(result);
         _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
         _context.ChangeTracker.Clear();
    }
    [Fact]
    public async Task GetUserFollowing_CannotFindUser_ShouldReturnEmptyList()
    {
        //Arrange 
        var username = "SomeUsername";
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);
        //Act
        var result = await _followingService.GetUserFollowing(username);
        //Assert
        Assert.Empty(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
        _context.ChangeTracker.Clear();
    }
    
}
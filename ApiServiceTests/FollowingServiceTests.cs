using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.User;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ApiServiceTests;

public class FollowingServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager;

    public FollowingServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!,
            null!, null!, null!);
    }

    [Fact]
    public async Task GetUserFollowersCount_ReturnsCorrectCounts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var userId = "user-1";
        var user = new ApplicationUser { Id = userId };

        context.FollowUsers.AddRange(
            new Followers { FollowerUserId = userId, FollowedUserId = "user-2" },
            new Followers { FollowerUserId = "user-3", FollowedUserId = userId },
            new Followers { FollowerUserId = "user-4", FollowedUserId = userId }
        );
        await context.SaveChangesAsync();

        _userManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

        var service = new FollowingService(_userManager.Object, context);

        // Act
        var result = await service.GetUserFollowersCount(userId);

        // Assert
        Assert.Equal(2, result.FollowersCount);
        Assert.Equal(1, result.FollowingCount);
    }

    [Fact]
    public async Task FollowUser_EmptyCurrentUsername_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var request = new FollowRequestDto("", "SomeUsername");
        var service = new FollowingService(_userManager.Object, context);

        //Act 
        var result = await service.FollowUser(request);

        //Assert 
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task FollowUser_EmptyTargetUsername_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var request = new FollowRequestDto("SomeUsername", "");
        var service = new FollowingService(_userManager.Object, context);

        //Act 
        var result = await service.FollowUser(request);

        //Assert 
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task FollowUser_CannotFindUser_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var request = new FollowRequestDto("SomeUsername", "SomeOtherUsername");
        var service = new FollowingService(_userManager.Object, context);
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);

        //Act
        var result = await service.FollowUser(request);

        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task FollowUser_GoodData_UserExists_ShouldReturnOk()
    {
        //Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new ApplicationUser { UserName = "Alice", Id = "1" };
        var targetUser = new ApplicationUser { UserName = "Bob", Id = "2" };
        await using var context = new ApplicationDbContext(options);
        var request = new FollowRequestDto(currentUser.UserName, targetUser.UserName);
        _userManager.Setup(x => x.FindByNameAsync("Alice")).ReturnsAsync(currentUser);
        _userManager.Setup(x => x.FindByNameAsync("Bob")).ReturnsAsync(targetUser);

        var service = new FollowingService(_userManager.Object, context);
        //Act 
        var result = await service.FollowUser(request);

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Flag);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
        var followEntry = await context.FollowUsers
            .FirstOrDefaultAsync(f => f.FollowerUserId == "1" && f.FollowedUserId == "2");
        Assert.NotNull(followEntry);
    }

    [Fact]
    public async Task FollowUser_GoodData_UserAlreadyFollowed_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new ApplicationUser { UserName = "Alice", Id = "1" };
        var targetUser = new ApplicationUser { UserName = "Bob", Id = "2" };
        await using var context = new ApplicationDbContext(options);

        context.FollowUsers.Add(new Followers() { Id = 1, FollowerUserId = "1", FollowedUserId = "2" });
        await context.SaveChangesAsync();

        var request = new FollowRequestDto(currentUser.UserName, targetUser.UserName);
        _userManager.Setup(x => x.FindByNameAsync("Alice")).ReturnsAsync(currentUser);
        _userManager.Setup(x => x.FindByNameAsync("Bob")).ReturnsAsync(targetUser);

        var service = new FollowingService(_userManager.Object, context);
        //Act 
        var result = await service.FollowUser(request);

        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
        var followEntry = await context.FollowUsers
            .FirstOrDefaultAsync(f => f.FollowerUserId == "1" && f.FollowedUserId == "2");
        Assert.NotNull(followEntry);
    }

    [Fact]
    public async Task UnFollowUser_EmptyCurrentUsername_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var request = new FollowRequestDto("", "SomeUsername");
        var service = new FollowingService(_userManager.Object, context);

        //Act 
        var result = await service.UnfollowUser(request);

        //Assert 
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UnFollowUser_EmptyTargetUsername_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var request = new FollowRequestDto("SomeUsername", "");
        var service = new FollowingService(_userManager.Object, context);

        //Act 
        var result = await service.UnfollowUser(request);

        //Assert 
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UnFollowUser_CannotFindUser_ShouldReturnBadServiceResponse()
    {
        //Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var request = new FollowRequestDto("SomeUsername", "SomeOtherUsername");
        var service = new FollowingService(_userManager.Object, context);
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);

        //Act
        var result = await service.UnfollowUser(request);

        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UnFollowUser_GoodData_UserExists_ShouldReturnOk()
    {
        //Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new ApplicationUser { UserName = "Alice", Id = "1" };
        var targetUser = new ApplicationUser { UserName = "Bob", Id = "2" };
        await using var context = new ApplicationDbContext(options);

        context.FollowUsers.Add(new Followers() { Id = 1, FollowerUserId = "1", FollowedUserId = "2" });
        await context.SaveChangesAsync();
        var request = new FollowRequestDto(currentUser.UserName, targetUser.UserName);
        _userManager.Setup(x => x.FindByNameAsync("Alice")).ReturnsAsync(currentUser);
        _userManager.Setup(x => x.FindByNameAsync("Bob")).ReturnsAsync(targetUser);

        var service = new FollowingService(_userManager.Object, context);
        //Act 
        var result = await service.UnfollowUser(request);

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Flag);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
        var followEntry = await context.FollowUsers
            .FirstOrDefaultAsync(f => f.FollowerUserId == "1" && f.FollowedUserId == "2");
        Assert.Null(followEntry);
    }

    [Fact]
    public async Task IsUserFollowing_NoCurrentUser_ShouldReturnFalse()
    {
        //Arrange
        var request = new FollowRequestDto("", "SomeOtherUsername");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new ApplicationDbContext(options);

        var service = new FollowingService(_userManager.Object, context);
        //Act
        var result = await service.IsUserFollowing(request);

        //Assert
        Assert.False(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task IsUserFollowing_NoTargetUser_ShouldReturnFalse()
    {
        //Arrange
        var request = new FollowRequestDto("SomeUsername", "");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new ApplicationDbContext(options);

        var service = new FollowingService(_userManager.Object, context);
        //Act
        var result = await service.IsUserFollowing(request);

        //Assert
        Assert.False(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task IsUserFollowing_UserDoesntExist_ShouldReturnFalse()
    {
        //Arrange
        var request = new FollowRequestDto("SomeUsername", "SomeOtherUsername");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new ApplicationDbContext(options);
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);
        var service = new FollowingService(_userManager.Object, context);
        //Act
        var result = await service.IsUserFollowing(request);

        //Assert
        Assert.False(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task IsUserFollowing_UserExists_IsNotFollowingUser_ShouldReturnFalse()
    {
        //Arrange
        var request = new FollowRequestDto("SomeUsername", "SomeOtherUsername");
        var currentUser = new ApplicationUser { UserName = "Alice", Id = "1" };
        var targetUser = new ApplicationUser { UserName = "Bob", Id = "2" };
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new ApplicationDbContext(options);
        _userManager.Setup(x => x.FindByNameAsync("Alice")).ReturnsAsync(currentUser);
        _userManager.Setup(x => x.FindByNameAsync("Bob")).ReturnsAsync(targetUser);
        var service = new FollowingService(_userManager.Object, context);
        //Act
        var result = await service.IsUserFollowing(request);

        //Assert
        Assert.False(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
    }
    [Fact]
    public async Task IsUserFollowing_UserExists_IsFollowingUser_ShouldReturnFalse()
    {
        //Arrange
        var request = new FollowRequestDto("Alice", "Bob");
        var currentUser = new ApplicationUser { UserName = "Alice", Id = "1" };
        var targetUser = new ApplicationUser { UserName = "Bob", Id = "2" };
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new ApplicationDbContext(options);
        context.FollowUsers.Add(new Followers() { Id = 1, FollowerUserId = "1", FollowedUserId = "2" });
        await context.SaveChangesAsync();
        _userManager.Setup(x => x.FindByNameAsync("Alice")).ReturnsAsync(currentUser);
        _userManager.Setup(x => x.FindByNameAsync("Bob")).ReturnsAsync(targetUser);
        var service = new FollowingService(_userManager.Object, context);
        //Act
        var result = await service.IsUserFollowing(request);

        //Assert
        Assert.True(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetUserFollowers_EmptyUser_ShouldReturnEmptyList()
    {
         //Arrange 
         var username = "";
         var options = new DbContextOptionsBuilder<ApplicationDbContext>()
             .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
             .Options;
         await using var context = new ApplicationDbContext(options);
         var service = new FollowingService(_userManager.Object, context);
         //Act
         var result = await service.GetUserFollowers(username);
         //Assert
         Assert.Empty(result);
         _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task GetUserFollowers_CannotFindUser_ShouldReturnEmptyList()
    {
        //Arrange 
        var username = "SomeUsername";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new ApplicationDbContext(options);
        var service = new FollowingService(_userManager.Object, context);
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);
        //Act
        var result = await service.GetUserFollowers(username);
        //Assert
        Assert.Empty(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public async Task GetUserFollowing_EmptyUser_ShouldReturnEmptyList()
    {
         //Arrange 
         var username = "";
         var options = new DbContextOptionsBuilder<ApplicationDbContext>()
             .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
             .Options;
         await using var context = new ApplicationDbContext(options);
         var service = new FollowingService(_userManager.Object, context);
         //Act
         var result = await service.GetUserFollowing(username);
         //Assert
         Assert.Empty(result);
         _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task GetUserFollowing_CannotFindUser_ShouldReturnEmptyList()
    {
        //Arrange 
        var username = "SomeUsername";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new ApplicationDbContext(options);
        var service = new FollowingService(_userManager.Object, context);
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);
        //Act
        var result = await service.GetUserFollowing(username);
        //Assert
        Assert.Empty(result);
        _userManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    }
    
}
using ApplicationLayer.APIServices;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.User;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ApiServiceTests;

public class FollowingServiceTests
{
    private readonly FollowingService _followingService;
    private readonly Mock<ApplicationDbContext> _dbContext;
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    
    public FollowingServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
    }
    [Fact]
    public async Task GetUserFollowersCount_ReturnsCorrectCounts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
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
        Assert.Equal(1, result.FollowersCount);
        Assert.Equal(2, result.FollowingCount);
    }

}
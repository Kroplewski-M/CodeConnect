using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.General;
using InfrastructureLayer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApiApplicationLayer.Services;

namespace ApiServiceTests;

public class NotificationsServiceTests: IDisposable
{
    private readonly Mock<INotificationHub> _mockClientProxy;
    private readonly ApplicationDbContext _inMemoryDbContext;
    private readonly NotificationService _notificationService;
    public NotificationsServiceTests()
    {
        var mockHubContext = new Mock<IHubContext<NotificationsHub, INotificationHub>>();
        var mockClients = new Mock<IHubClients<INotificationHub>>();
        _mockClientProxy = new Mock<INotificationHub>();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);
        mockClients.Setup(x => x.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _inMemoryDbContext = new ApplicationDbContext(options);
        
        _notificationService = new NotificationService(mockHubContext.Object, _inMemoryDbContext);
    }
    public void Dispose()
    {
        _inMemoryDbContext.Dispose();
    }
    [Fact]
    public async Task SendNotificationAsync_ShouldNotSendNotification_WhenForUserIdIsSameAsFromUserId()
    {
        string userId = "user1";

        await _notificationService.SendNotificationAsync(userId, userId, Consts.NotificationTypes.CommentLike, "target1");

        Assert.Empty(_inMemoryDbContext.UserNotifications);
    }
    [Fact]
    public async Task SendNotificationAsync_ShouldNotSendNotification_WhenNotificationExists()
    {
        _inMemoryDbContext.UserNotifications.Add(new UserNotification
        {
            ForUserId = "forUser",
            FromUserId = "fromUser",
            NotificationTypeId = (int)Consts.NotificationTypes.CommentLike,
            TargetId = "target1"
        });
        await _inMemoryDbContext.SaveChangesAsync();

        await _notificationService.SendNotificationAsync("forUser", "fromUser", Consts.NotificationTypes.CommentLike, "target1");

        Assert.Single(_inMemoryDbContext.UserNotifications);
    }
    [Fact]
    public async Task SendNotificationAsync_ShouldSendNotification_WhenNotificationDoesNotExist()
    {
        await _notificationService.SendNotificationAsync("forUser", "fromUser", Consts.NotificationTypes.CommentLike, "target1");

        var notification = _inMemoryDbContext.UserNotifications.FirstOrDefault();
        var notificationCount = _inMemoryDbContext.UserNotifications.Count();
        Assert.NotNull(notification);
        Assert.Equal("forUser", notification.ForUserId);
        Assert.Equal("fromUser", notification.FromUserId);
        Assert.Equal((int)Consts.NotificationTypes.CommentLike, notification.NotificationTypeId);
        Assert.Equal("target1", notification.TargetId);
        Assert.Equal(notificationCount, _inMemoryDbContext.UserNotifications.Count());
        _mockClientProxy.Verify(x => x.NotificationPing(), Times.Once);
    }
    [Fact]
    public async Task GetUsersNotificationsCount_ShouldReturnZero_WhenUserIdIsNull()
    {
        int result = await _notificationService.GetUsersNotificationsCount(null);

        Assert.Equal(0, result);
    }
    [Fact]
    public async Task GetUsersNotificationsCount_ShouldReturnCorrectCount_WhenUserIdIsValid()
    {
        var userId = Guid.NewGuid().ToString();
        
        _inMemoryDbContext.UserNotifications.AddRange(new UserNotification
        {
            FromUserId = "userId1",
            ForUserId = userId,
            IsRead = false,
            TargetId = "target",
        }, new UserNotification
        {
            FromUserId = "userId1",
            ForUserId = userId,
            IsRead = false,
            TargetId = "target",
        });
        await _inMemoryDbContext.SaveChangesAsync();

        int result = await _notificationService.GetUsersNotificationsCount(userId);

        Assert.Equal(2, result);
    }
    [Fact]
    public async Task GetUsersNotifications_ShouldReturnEmptyList_WhenUserIdIsNull()
    {
        var result = await _notificationService.GetUsersNotifications(null);

        Assert.False(result.Flag);
        Assert.Empty(result.Notifications);
    }
    [Fact]
    public async Task GetUsersNotifications_ShouldReturnCorrectNotifications_WhenUserIdIsValid()
    {
        var userId = Guid.NewGuid().ToString();
        var fromUser = new ApplicationUser
        {
            Id = userId,
            UserName = "testuser",
        };
        _inMemoryDbContext.Users.Add(fromUser);
        _inMemoryDbContext.UserNotifications.Add(new UserNotification
        {
            Id = Guid.NewGuid(),
            FromUserId = userId,
            NotificationTypeId = (int)Consts.NotificationTypes.CommentLike,
            TargetId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ForUserId = userId,
            IsRead = false,
        });
        await _inMemoryDbContext.SaveChangesAsync();
        var t = _inMemoryDbContext.UserNotifications.ToList();
        var result = await _notificationService.GetUsersNotifications(userId);

        Assert.True(result.Flag);
        Assert.Single(result.Notifications);
    }
    [Fact]
    public async Task MarkNotificationAsRead_ShouldReturnFailure_WhenUserIdIsNull()
    {
        var result = await _notificationService.MarkNotificationAsRead(Guid.NewGuid(), null);

        Assert.False(result.Flag);
        Assert.Equal("Error occured while marking notification as read", result.Message);
    }
    [Fact]
    public async Task MarkNotificationAsRead_ShouldMarkAsRead_WhenNotificationExists()
    {
        var notification = new UserNotification
        {
            Id = Guid.NewGuid(),
            ForUserId = "userId",
            FromUserId = "user2",
            TargetId = "target",
            IsRead = false
        };

        _inMemoryDbContext.UserNotifications.Add(notification);
        await _inMemoryDbContext.SaveChangesAsync();

        var result = await _notificationService.MarkNotificationAsRead(notification.Id, "userId");

        Assert.True(result.Flag);

        var updatedNotification = _inMemoryDbContext.UserNotifications.FirstOrDefault(x => x.Id == notification.Id);
        Assert.NotNull(updatedNotification);
        Assert.True(updatedNotification.IsRead);
    }
    [Fact]
    public async Task MarkAllNotificationsAsRead_ShouldReturnFailure_WhenUserIdIsNull()
    {
        var result = await _notificationService.MarkAllNotificationsAsRead(null);

        Assert.False(result.Flag);
        Assert.Equal("Error occured while marking notification as read", result.Message);
    }
}
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using CodeConnect.WebAPI.Endpoints.NotificationsEndpoint;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApiApplicationLayer.Interfaces;

namespace TestLayer;

public class NotificationsControllerTests
{
    private readonly Mock<IServerNotificationsService> _mockNotificationsService;
    private readonly NotificationsController _notificationsController;
    public NotificationsControllerTests()
    {
        _mockNotificationsService = new Mock<IServerNotificationsService>();
        _notificationsController = new NotificationsController(_mockNotificationsService.Object);

        ControllerTestHelper.SetMockUserInContext(_notificationsController, "user", Guid.NewGuid().ToString());
    }
    [Fact]
    public async Task GetNotificationsCount_ShouldReturnOk_WhenUserIsValid()
    {
        // Arrange
        var expectedCount = 5;
        _mockNotificationsService.Setup(s => s.GetUsersNotificationsCount(It.IsAny<string>()))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _notificationsController.GetNotificationsCount();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult); 
        Assert.Equal(expectedCount, okResult!.Value); 

        _mockNotificationsService.Verify(s => s.GetUsersNotificationsCount(It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public async Task GetNotificationsCount_ShouldReturnBadRequest_WhenUserIsNotThere()
    {
        // Act
        ControllerTestHelper.SetMockUserInContext(_notificationsController, "", "");
        var result = await _notificationsController.GetNotificationsCount();

        // Assert
        var badRequestObjectResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestObjectResult); 

        _mockNotificationsService.Verify(s => s.GetUsersNotificationsCount(It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task GetNotifications_ShouldReturnOk_WhenUserIsValid()
    {
        // Arrange
        var expected = new GetNotificationsDto(true, new List<NotificationsDto>());
        _mockNotificationsService.Setup(s => s.GetUsersNotifications(It.IsAny<string>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _notificationsController.GetNotifications();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult); 
        var response = Assert.IsType<GetNotificationsDto>(okResult.Value);
        Assert.True(response.Flag);

        _mockNotificationsService.Verify(s => s.GetUsersNotifications(It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public async Task GetNotifications_ShouldReturnBadRequest_WhenUserIsNotThere()
    {
        // Act
        ControllerTestHelper.SetMockUserInContext(_notificationsController, "", "");
        var result = await _notificationsController.GetNotifications();

        // Assert
        var badRequestObjectResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestObjectResult); 

        _mockNotificationsService.Verify(s => s.GetUsersNotificationsCount(It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task MarkNotificationAsRead_ShouldReturnOk_WhenNotificationMarkedSuccessfully()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var serviceResult = new ServiceResponse(true, "");
        _mockNotificationsService.Setup(s => s.MarkNotificationAsRead(notificationId, It.IsAny<string>()))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _notificationsController.MarkNotificationAsRead(notificationId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult); 
        var response = Assert.IsType<ServiceResponse>(okResult.Value);
        Assert.True(response.Flag);
        _mockNotificationsService.Verify(s => s.MarkNotificationAsRead(notificationId, It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public async Task MarkNotificationAsRead_ShouldReturnBadRequest_WhenNoUser()
    {
        // Arrange
        ControllerTestHelper.SetMockUserInContext(_notificationsController, "", "");
        var notificationId = Guid.NewGuid();
        var serviceResult = new ServiceResponse(true, "");
        _mockNotificationsService.Setup(s => s.MarkNotificationAsRead(notificationId, It.IsAny<string>()))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _notificationsController.MarkNotificationAsRead(notificationId);

        // Assert
        var badRequestObjectResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestObjectResult); 
        var response = Assert.IsType<ServiceResponse>(badRequestObjectResult.Value);
        Assert.False(response.Flag);
        _mockNotificationsService.Verify(s => s.MarkNotificationAsRead(notificationId, It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task MarkNotificationAsRead_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var serviceResult = new ServiceResponse(false, "");
        _mockNotificationsService.Setup(s => s.MarkNotificationAsRead(notificationId, It.IsAny<string>()))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _notificationsController.MarkNotificationAsRead(notificationId);

        // Assert
        var badRequestObjectResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestObjectResult); 
        var response = Assert.IsType<ServiceResponse>(badRequestObjectResult.Value);
        Assert.False(response.Flag);
        _mockNotificationsService.Verify(s => s.MarkNotificationAsRead(notificationId, It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public async Task MarkAllNotificationsAsRead_ShouldReturnOk_WhenAllMarkedSuccessfully()
    {
        // Arrange
        var serviceResult = new ServiceResponse(true, "");
        _mockNotificationsService.Setup(s => s.MarkAllNotificationsAsRead(It.IsAny<string>()))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _notificationsController.MarkAllNotificationsAsRead();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult); 
        var response = Assert.IsType<ServiceResponse>(okResult.Value);
        Assert.True(response.Flag);
        _mockNotificationsService.Verify(s => s.MarkAllNotificationsAsRead(It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public async Task MarkAllNotificationsAsRead_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        var serviceResult = new ServiceResponse(false, "");
        _mockNotificationsService.Setup(s => s.MarkAllNotificationsAsRead(It.IsAny<string>()))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _notificationsController.MarkAllNotificationsAsRead();

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult); 
        var response = Assert.IsType<ServiceResponse>(badRequestResult.Value);
        Assert.False(response.Flag);

        _mockNotificationsService.Verify(s => s.MarkAllNotificationsAsRead(It.IsAny<string>()), Times.Once);
    }
}
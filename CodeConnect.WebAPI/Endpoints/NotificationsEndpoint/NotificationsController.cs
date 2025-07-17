using ApplicationLayer.ExtensionClasses;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiApplicationLayer.Interfaces;

namespace CodeConnect.WebAPI.Endpoints.NotificationsEndpoint;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController(IServerNotificationsService notificationsService): ControllerBase
{
    [Authorize(nameof(Consts.TokenType.Access))]
    [HttpGet("GetNotificationsCount")]
    public async Task<IActionResult> GetNotificationsCount()
    {
        var userId = User.GetInfo(Consts.ClaimTypes.Id);
        if(string.IsNullOrWhiteSpace(userId))
            return BadRequest("User not found");
        return Ok(await notificationsService.GetUsersNotificationsCount(userId));
    }

    [Authorize(nameof(Consts.TokenType.Access))]
    [HttpGet("GetNotifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = User.GetInfo(Consts.ClaimTypes.Id);
        if(string.IsNullOrWhiteSpace(userId))
            return BadRequest("User not found");
        return Ok(await notificationsService.GetUsersNotifications(userId));
    }
    [Authorize(nameof(Consts.TokenType.Access))]
    [HttpPost("MarkNotificationAsRead")]
    public async Task<IActionResult> MarkNotificationAsRead([FromBody]Guid notificationId)
    {
        var userId = User.GetInfo(Consts.ClaimTypes.Id);
        if(string.IsNullOrWhiteSpace(userId))
            return BadRequest("User not found");
        var result = await notificationsService.MarkNotificationAsRead(notificationId, userId);
        if(result.Flag)
            return Ok(result);
        return BadRequest(result);
    }
    [Authorize(nameof(Consts.TokenType.Access))]
    [HttpPost("MarkAllNotificationsAsRead")]
    public async Task<IActionResult> MarkAllNotificationsAsRead()
    {
        var userId = User.GetInfo(Consts.ClaimTypes.Id);
        if(string.IsNullOrWhiteSpace(userId))
            return BadRequest("User not found");
        var result = await notificationsService.MarkAllNotificationsAsRead(userId);
        if(result.Flag)
            return Ok(result);
        return BadRequest(result);
    }
}
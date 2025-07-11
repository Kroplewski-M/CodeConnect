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
}
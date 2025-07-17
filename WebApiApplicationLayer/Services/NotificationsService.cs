using ApplicationLayer.DTO_s;
using ApplicationLayer.ExtensionClasses;
using DomainLayer.Constants;
using DomainLayer.Entities.General;
using InfrastructureLayer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApiApplicationLayer.Interfaces;

namespace WebApiApplicationLayer.Services;

public class NotificationService(IHubContext<NotificationsHub, INotificationHub> hubContext,ApplicationDbContext context) : IServerNotificationsService
{
    public async Task SendNotificationAsync(string forUserId, string fromUserId, Consts.NotificationTypes notificationType, string targetId)
    {
        if(forUserId == fromUserId)
            return;
        bool notificationExists = NotificationExists(forUserId, fromUserId, notificationType, targetId);
        if (notificationExists)
            return;
        UserNotification userNotification = new UserNotification()
        {
            ForUserId = forUserId,
            FromUserId = fromUserId,
            NotificationTypeId = (int)notificationType,
            TargetId = targetId,
            CreatedAt = DateTime.UtcNow,
        };
        context.UserNotifications.Add(userNotification);
        await context.SaveChangesAsync();
        await hubContext.Clients.User(forUserId).NotificationPing();
    }
    
    public bool NotificationExists(string forUserId, string fromUserId, Consts.NotificationTypes notificationType, string targetId)
    {
        return context.UserNotifications.Any(x => x.FromUserId == fromUserId
                                                  && x.ForUserId == forUserId
                                                  && x.TargetId == targetId
                                                  && x.NotificationTypeId == (int)notificationType);
    }

    public async Task<int> GetUsersNotificationsCount(string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return 0;
        return await GetUserNotifications(userId)
            .CountAsync();
    }

    public async Task<GetNotificationsDto> GetUsersNotifications(string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new GetNotificationsDto(false, new List<NotificationsDto>());
        var rawNotifications = await GetUserNotifications(userId)
            .Select(x => new
            {
                x.Id,
                x.FromUser,
                NotificationType = x.NotificationTypeId,
                x.TargetId,
                x.CreatedAt,
            }).ToListAsync();
            
         var notifications= rawNotifications
             .Select(x=> new NotificationsDto(x.Id,x.FromUser.ToUserBasicDto(), (Consts.NotificationTypes)x.NotificationType, x.CreatedAt, x.TargetId))
            .ToList();
        return new GetNotificationsDto(true, notifications);
    }

    public async Task<ServiceResponse> MarkNotificationAsRead(Guid notificationId, string? userId = null)
    {
        if(userId == null)
            return new ServiceResponse(false, "Error occured while marking notification as read");
        var notification = await context.UserNotifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.ForUserId == userId);
        if(notification == null)
            return new ServiceResponse(false, "Notification not found");
        notification.IsRead = true;
        await context.SaveChangesAsync();
        return new ServiceResponse(true, "Notification marked as read");
    }

    private IQueryable<UserNotification> GetUserNotifications(string userId)
        => context.UserNotifications
            .Where(x => x.ForUserId == userId && !x.IsRead);

    public async Task<ServiceResponse> MarkAllNotificationsAsRead(string? userId = null)
    {
        if(userId == null)
            return new ServiceResponse(false, "Error occured while marking notification as read");
        await context.UserNotifications
            .Where(x => x.ForUserId  == userId && !x.IsRead)
            .ExecuteUpdateAsync(setters => 
                setters.SetProperty(n => n.IsRead, true));
        await context.SaveChangesAsync();
        return new ServiceResponse(true, "All notifications marked as read");
    }
}
using ApplicationLayer.DTO_s;

namespace ApplicationLayer.Interfaces;

public interface ISharedNotificationsService
{
    public Task<int> GetUsersNotificationsCount(string? userId = null);
    public Task<GetNotificationsDto> GetUsersNotifications(string? userId = null);
    public Task<ServiceResponse> MarkNotificationAsRead(Guid notificationId, string? userId = null);
    public Task<ServiceResponse> MarkAllNotificationsAsRead(string? userId = null);
}
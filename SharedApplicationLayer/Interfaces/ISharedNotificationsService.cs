namespace ApplicationLayer.Interfaces;

public interface ISharedNotificationsService
{
    public Task<int> GetUsersNotificationsCount(string? userId = null);
}
using ApplicationLayer;
using ApplicationLayer.DTO_s;
using ClientApplicationLayer.Interfaces;
using ClientApplicationLayer.Services;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages.Notifications;

public class NotificationsBase : ComponentBase
{
    [Inject] public required IClientNotificationsService NotificationsService { get; set; }
    [Inject] public required ToastService ToastService { get; set; }
    protected List<NotificationsDto> Notifications { get; set; } = new List<NotificationsDto>();
    protected override async Task OnInitializedAsync()
    {
        var notifications = await NotificationsService.GetUsersNotifications();
        if(notifications.Flag)
            Notifications = notifications.Notifications;
        else
            ToastService.PushToast(new Toast("An error occured while fetching notifications", ToastType.Error));
    }

    protected async Task ClearAllNotifications()
    {
        var res = await NotificationsService.MarkAllNotificationsAsRead();
        if(res.Flag)
            Notifications.Clear();
        else
            ToastService.PushToast(new Toast("An error occured while clearing notifications", ToastType.Error));
        StateHasChanged();
    }
    public async Task ClearNotification((Guid notificationId, bool stateChanged) data)
    {
        await NotificationsService.MarkNotificationAsRead(data.notificationId);
        if (data.stateChanged)
        {
            Notifications.RemoveAll(x => x.Id == data.notificationId);
            StateHasChanged();
        }
    }
}
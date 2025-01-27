using ApplicationLayer;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class UserFollowingBase : ComponentBase
{
    [Inject]
    public required IFollowingService FollowingService { get; set; }
    [Inject]
    public required NavigationManager NavigationManager { get; set; }
    [Inject]
    public required NotificationsService NotificationsService { get; set; }
    public List<UserBasicDto> Followers { get; set; } = new List<UserBasicDto>();

    public bool Loading { get; set; } = true;
    protected override async Task OnParametersSetAsync()
    {
        //Get username from URL
        var uri = NavigationManager.Uri;
        var segments = new Uri(uri).Segments;
        var username = segments.LastOrDefault();
        if (username != null)
        {
            Followers = await FollowingService.GetUserFollowing(username);
        }
        else
        {
            NotificationsService.PushNotification(new ApplicationLayer.Notification("Error occured during fetching users", NotificationType.Error));
        }
        Loading = false;
        StateHasChanged();
    }
}
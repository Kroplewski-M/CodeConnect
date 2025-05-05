using ApplicationLayer;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class UserFollowersBase() : ComponentBase
{
    [Inject] public required IFollowingService FollowingService { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required NotificationsService NotificationsService { get; set; }
    [CascadingParameter] public required string Username { get; set; }
    public List<UserBasicDto> Followers { get; set; } = new List<UserBasicDto>();

    public bool Loading { get; set; } = true;
    protected override async Task OnParametersSetAsync()
    {
        //Get username from URL

        if (!string.IsNullOrWhiteSpace(Username))
        {
            Followers = await FollowingService.GetUserFollowers(Username);
        }
        else
        {
            NotificationsService.PushNotification(new ApplicationLayer.Notification("Error occured during fetching users", NotificationType.Error));
        }
        Loading = false;
        StateHasChanged();
    }
}
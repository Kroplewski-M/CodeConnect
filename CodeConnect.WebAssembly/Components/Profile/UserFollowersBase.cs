using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class UserFollowersBase() : ComponentBase
{
    [Inject]
    public required IFollowingService FollowingService { get; set; }
    [Inject]
    public required NavigationManager NavigationManager { get; set; }
    public List<UserBasicDto> Followers { get; set; } = new List<UserBasicDto>();

    public bool Loading { get; set; } = true;
    protected override async Task OnParametersSetAsync()
    {
        //Get username from URL
        var uri = NavigationManager.Uri;
        var segments = new Uri(uri).Segments;
        var username = segments.LastOrDefault();
        Followers = await FollowingService.GetUserFollowers(username);
        Loading = false;
        StateHasChanged();
    }
}
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class UserFollowersBase() : ComponentBase
{
    [Inject]
    public required IFollowingService FollowingService { get; set; }
    public List<UserBasicDto> Followers { get; set; } = new List<UserBasicDto>();

    public bool Loading { get; set; } = true;
    protected override async Task OnInitializedAsync()
    {
        Followers = await FollowingService.GetUserFollowers(null);
        Loading = false;
    }
}
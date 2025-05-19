using ApplicationLayer;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class UserFollowingBase : ComponentBase
{
    [Inject] public required IFollowingService FollowingService { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required NotificationsService NotificationsService { get; set; }
    [CascadingParameter] public required string Username { get; set; }
    public List<UserBasicDto> Following { get; set; } = new List<UserBasicDto>();
    
    protected async Task LoadMoreFollowing((int,int)range)
    {
        var (startIndex, take) = range;
        var more = await FollowingService.GetUserFollowing(Username, skip: startIndex, take: take);
        if (more?.Any() == true)
        {
            Following.AddRange(more);
        }
    }
}
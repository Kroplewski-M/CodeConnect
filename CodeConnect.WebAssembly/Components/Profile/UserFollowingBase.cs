using ApplicationLayer;
using ApplicationLayer.Classes;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class UserFollowingBase : ComponentBase
{
    [Inject] public required IFollowingService FollowingService { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required NotificationsService NotificationsService { get; set; }
    [CascadingParameter] public required UserState UserState { get; set; }
    public List<UserBasicDto> Following { get; set; } = new List<UserBasicDto>();
    
    protected async Task LoadMoreFollowing((int,int)range)
    {
        if(UserState.Current == null) return;
        var (startIndex, take) = range;
        var more = await FollowingService.GetUserFollowing(UserState.Current.UserName, skip: startIndex, take: take);
        if (more?.Any() == true)
        {
            Following.AddRange(more);
        }
    }
}
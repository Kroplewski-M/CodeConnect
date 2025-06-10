using ApplicationLayer;
using ApplicationLayer.Classes;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace CodeConnect.WebAssembly.Components.Profile;

public class UserFollowersBase() : ComponentBase
{
    [Inject] public required IFollowingService FollowingService { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required NotificationsService NotificationsService { get; set; }
    [CascadingParameter] public required string Username { get; set; }
    public List<UserBasicDto> Followers { get; set; } = new List<UserBasicDto>();
    
    protected async Task LoadMoreFollowers((int,int)range)
    {
        var (startIndex, take) = range;
        var more = await FollowingService.GetUserFollowers(Username, skip: startIndex, take: take);
        if (more?.Any() == true)
        {
            Followers.AddRange(more);
        }
    }
}
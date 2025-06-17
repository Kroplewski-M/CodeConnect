using ApplicationLayer.Classes;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class FollowUserBase : ComponentBase
{
    [Inject] public required IFollowingService FollowingService { get; set; }
    protected bool Following { get; set; }
    [CascadingParameter] public required UserState UserState { get; set; }
    [Parameter] public required string FollowUsername { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (UserState.Current == null) return;
        Following = await FollowingService.IsUserFollowing(new FollowRequestDto(UserState.Current.UserName, FollowUsername));
    }

    protected string UnFollowText => $"Are you sure you want to unfollow {FollowUsername}?";
    protected bool DisableFollow { get; set; } = false;
    protected async Task ToggleFollow()
    {
        if (UserState.Current == null) return;
        DisableFollow = true;
        var request = new FollowRequestDto(UserState.Current.UserName, FollowUsername);
        if (!Following)
        {
            await FollowingService.FollowUser(request);
            Following = true;
        }
        else
            ConfirmUnFollow = true;
        DisableFollow = false;
        await InvokeAsync(StateHasChanged);
    }

    protected bool ConfirmUnFollow { get; set; } = false;

    protected async Task UnFollow()
    {
        if (UserState.Current == null) return;
        DisableFollow = true;
        var request = new FollowRequestDto(UserState.Current.UserName, FollowUsername);
        await FollowingService.UnfollowUser(request);
        Following = !Following;
        ConfirmUnFollow = false;
        DisableFollow = false;
        await InvokeAsync(StateHasChanged);
    }

    protected void CancelUnFollow()
    {
        ConfirmUnFollow = false;
        StateHasChanged();
    }
}
    
    
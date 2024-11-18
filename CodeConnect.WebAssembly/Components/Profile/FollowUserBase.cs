using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class FollowUserBase : ComponentBase
{
    [Inject] public required IFollowingService FollowingService { get; set; }
    protected bool Following { get; set; }
    [Parameter] public required string CurrentUsername { get; set; }
    [Parameter] public required string FollowUsername { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Following = await FollowingService.IsUserFollowing(new FollowRequestDto(CurrentUsername, FollowUsername));
    }

    protected bool DisableFollow { get; set; } = false;
    protected async Task ToggleFollow()
    {
        DisableFollow = true;
        var request = new FollowRequestDto(CurrentUsername, FollowUsername);
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
        DisableFollow = true;
        var request = new FollowRequestDto(CurrentUsername, FollowUsername);
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
    
    
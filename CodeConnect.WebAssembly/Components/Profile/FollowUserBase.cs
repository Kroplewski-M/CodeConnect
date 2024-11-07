using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class FollowUserBase : ComponentBase
{
    [Inject] public required IUserService UserService { get; set; }
    protected bool Following { get; set; } = false;
    [Parameter] public required string CurrentUsername { get; set; }
    [Parameter] public required string FollowUsername { get; set; }

    protected override async Task OnInitializedAsync()
    {
        //FETCH IF CURRENT USER IS FOLLIWING THIS USER
    }

    protected async Task ToggleFollow()
    {
        var request = new FollowRequestDto(CurrentUsername, FollowUsername);
        if (!Following)
        {
            await UserService.FollowUser(request);
        }

        Following = !Following;
        if (!Following)
            ConfirmUnFollow = true;
        await InvokeAsync(StateHasChanged);
    }

    protected bool ConfirmUnFollow { get; set; } = false;

    protected async Task UnFollow()
    {
        var request = new FollowRequestDto(CurrentUsername, FollowUsername);
        await UserService.FollowUser(request);
        Following = !Following;
        await InvokeAsync(StateHasChanged);
    }

    protected void CancelUnFollow() => ConfirmUnFollow = false;
}
    
    
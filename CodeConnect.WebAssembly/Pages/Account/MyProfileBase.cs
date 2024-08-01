using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CodeConnect.WebAssembly.Pages.Account;

public class MyProfileBase : ComponentBase
{
    [Inject]
    public required  IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    protected bool ShowConfirmLogout = false;
    protected bool ShowEditProfile = false;
    protected UserDetails? _userDetails = null;
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            var user = authState?.User;

            if (user?.Identity is not null && user.Identity.IsAuthenticated)
            {
                _userDetails = AuthenticateServiceClient.GetUserFromFromAuthState(authState);
                StateHasChanged();
            }
        }
    }
    protected void ToggleShowConfirmLogout()
    {
        ShowConfirmLogout = !ShowConfirmLogout;
        StateHasChanged();
    }
    protected void ToggleEditProfile()
    {
        ShowEditProfile = !ShowEditProfile;
        StateHasChanged();
    }
}
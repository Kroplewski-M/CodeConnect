using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using CodeConnect.WebAssembly.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CodeConnect.WebAssembly.Pages.Account;

public class ProfileBase : ComponentBase
{
    [Inject]
    public required  IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    [Parameter]
    public string? Username { get; set; }
    protected bool ShowConfirmLogout = false;
    protected bool ShowEditProfile = false;
    protected UserDetails? UserDetails = null;
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
                UserDetails = AuthenticateServiceClient.GetUserFromFromAuthState(authState);
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
    public TypeOfUpdate UpdateType { get; set; }
    protected bool ShowUpdateImage { get; set; }
    protected void UpdateImage(TypeOfUpdate imageType)
    {
        UpdateType = imageType;
        ShowUpdateImage = true;
        StateHasChanged();
    }

    protected void ToggleEditImage()
    {
        ShowUpdateImage = !ShowUpdateImage;
    }
}
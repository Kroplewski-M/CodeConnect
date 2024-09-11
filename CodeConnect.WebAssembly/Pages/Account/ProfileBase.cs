using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using CodeConnect.WebAssembly.Components;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CodeConnect.WebAssembly.Pages.Account;

public class ProfileBase : ComponentBase
{
    [Inject]
    public required  IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    [Inject]
    public required IUserService UserService { get; set; }
    [Parameter]
    public string? Username { get; set; }
    protected bool ShowConfirmLogout = false;
    protected bool ShowEditProfile = false;
    protected bool IsCurrentUser = false;
    protected bool FoundUser = false;
    protected UserDetails? UserDetails = null;
    protected UserInterestsDto? UserInterests { get; set; }

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
                var currentUser = AuthenticateServiceClient.GetUserFromFromAuthState(authState);
                if (currentUser.UserName == Username)
                {
                    IsCurrentUser = true;
                    UserDetails = currentUser;
                }
                else
                {
                    IsCurrentUser = false;
                    UserDetails = await UserService.GetUserDetails(Username ?? "");
                }

                if (UserDetails != null)
                {
                    UserInterests = await UserService.GetUserInterests(UserDetails.UserName);
                    FoundUser = true;
                }
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
    public Constants.ImageTypeOfUpdate UpdateImageType { get; set; }
    protected bool ShowUpdateImage { get; set; }
    protected void UpdateImage(Constants.ImageTypeOfUpdate imageImageType)
    {
        UpdateImageType = imageImageType;
        ShowUpdateImage = true;
        StateHasChanged();
    }

    protected void ToggleEditImage()
    {
        ShowUpdateImage = !ShowUpdateImage;
    }
}
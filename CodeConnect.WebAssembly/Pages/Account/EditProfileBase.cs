using ApplicationLayer.ClientServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CodeConnect.WebAssembly.Pages.Account;

public class EditProfileBase : ComponentBase
{
    [Inject]
    public required  IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    
    protected EditProfileForm EditProfileForm = new EditProfileForm();
    private UserDetails _userDetails;

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
                EditProfileForm = new EditProfileForm
                {
                    FirstName = _userDetails.firstName,
                    LastName = _userDetails.lastName,
                    DOB = _userDetails.DOB,
                    Bio = _userDetails.bio,
                    ProfileImgUrl = "",
                    ProfileImgExtention = "",
                    BackgroundImgUrl = "",
                    BackgoundImgExtention = ""
                };
                StateHasChanged();
            }
        }
    }
    protected void ConfirmEditProfile()
    {
        
    }
    
}
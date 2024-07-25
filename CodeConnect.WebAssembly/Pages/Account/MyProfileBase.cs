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
    protected string FirstName = "";
    protected string LastName = "";
    protected string ImgUrl = "";
    protected UserDetails _userDetails { get; set; }
    protected override async Task OnInitializedAsync()
    {
        _userDetails = AuthenticateServiceClient.GetUserDetails();
        StateHasChanged();
    }
    protected void ToggleShowConfirmLogout()
    {
        ShowConfirmLogout = !ShowConfirmLogout;
        StateHasChanged();
    }
}
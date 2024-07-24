using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CodeConnect.WebAssembly.Pages.Account;

public class MyProfileBase : ComponentBase
{
    [Inject]
    public required IAuthenticateService AuthenticateService { get; set; }
    [Inject]
    public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    protected bool ShowConfirmLogout = false;
    protected string FirstName = "";
    protected string LastName = "";
    protected string ImgUrl = "";
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        FirstName = user.FindFirst(c => c.Type == "FirstName")?.Value ?? "Undefined";
        LastName = user.FindFirst(c => c.Type == "LastName")?.Value ?? "Undefined";
        ImgUrl = String.IsNullOrEmpty(user.FindFirst(c => c.Type == "ProfileImg")?.Value) ? "images/profileImg.jpg" : user.FindFirst(c => c.Type == "ProfileImg")?.Value ?? "";
        StateHasChanged();
    }
    protected void ToggleShowConfirmLogout()
    {
        ShowConfirmLogout = !ShowConfirmLogout;
        StateHasChanged();
    }
}
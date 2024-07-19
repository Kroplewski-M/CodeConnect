using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages.Account;

public class MyProfileBase : ComponentBase
{
    [Inject]
    public required IAuthenticateService AuthenticateService { get; set; }

    protected bool ShowConfirmLogout = false;

    protected void ToggleShowConfirmLogout()
    {
        ShowConfirmLogout = !ShowConfirmLogout;
        StateHasChanged();
    }
}
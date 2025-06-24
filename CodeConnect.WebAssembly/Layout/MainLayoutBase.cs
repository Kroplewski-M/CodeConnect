using ApplicationLayer;
using ApplicationLayer.Classes;
using ApplicationLayer.ClientServices;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Layout;

public class MainLayoutBase : LayoutComponentBase, IDisposable
{
    [Inject] public required NavigationManager NavManager { get; set; }
    [Inject] public required NotificationsService NotificationsService { get; set; }
    [Inject] public required IJSRuntime Js { get; set; }
    [Inject] public required ILocalStorageService LocalStorageService { get; set; }
    [Inject] public required IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationState { get; set; }

    protected List<Notification> Notifications = [];
    protected bool DarkTheme = false;
    public readonly UserState UserState = new();

    protected override async Task OnInitializedAsync()
    {
        NotificationsService.OnChange += UpdateNotifications;
        Notifications = NotificationsService.GetNotification();
        AuthenticateServiceClient.OnChange += OnAuthenticationStateChanged;
        OnAuthenticationStateChanged();
        var darkTheme = await LocalStorageService.GetItemAsync<bool>("DarkTheme");
        if (darkTheme)
        {
            await ToggleDarkMode();
        }
    }

    protected bool IsNonNavPage()
    {
        string[] nonNavPages = ["/", "/Account/Register", "/Account/Login", "/404"];
        return nonNavPages.Any(page => NavManager.Uri.EndsWith(page));
    }

    protected void UpdateNotifications()
    {
        Notifications = NotificationsService.GetNotification();
        StateHasChanged();
    }

    public void Dispose()
    {
        NotificationsService.OnChange -= UpdateNotifications;
        AuthenticateServiceClient.OnChange -= OnAuthenticationStateChanged;
    }

    protected async Task ToggleDarkMode()
    {
        await Js.InvokeVoidAsync("toggleDarkMode");
        DarkTheme = !DarkTheme;
    }

    protected async Task SetDarkTheme(bool isDarkTheme)
    {
        await LocalStorageService.SetItemAsync<bool>("DarkTheme", isDarkTheme);
        await Js.InvokeVoidAsync("toggleDarkMode");
        DarkTheme = isDarkTheme;
    }

    protected bool HasDob { get; set; }

    private void OnAuthenticationStateChanged()
    {
        Task.Run(async () =>
        {
            if (AuthenticationState == null) return;
            var authState = await AuthenticationState;
            var user = authState?.User;

            if (user?.Identity is not null && user.Identity.IsAuthenticated)
            {
                var currentUser = await AuthenticateServiceClient.GetUserFromFromAuthState(authState);
                if (currentUser != UserState.Current)
                {
                    UserState.Current = currentUser;
                    HasDob = currentUser?.Dob != null;
                    StateHasChanged();
                }
            }
            else
            {
                UserState.Current = null;
                NavManager.NavigateTo("/Account/Login");
                StateHasChanged();
            }
        });
    }
}
using ApplicationLayer;
using ApplicationLayer.Classes;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using ClientApplicationLayer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Layout;

public class MainLayoutBase : LayoutComponentBase, IDisposable
{
    [Inject] public required NavigationManager NavManager { get; set; }
    [Inject] public required ToastService ToastService { get; set; }
    [Inject] public required IJSRuntime Js { get; set; }
    [Inject] public required ILocalStorageService LocalStorageService { get; set; }
    [Inject] public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] public required IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    [Inject] public required IUserService UserService { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationState { get; set; }

    protected List<Toast> Notifications = [];
    protected bool DarkTheme = false;
    public readonly UserState UserState = new();

    protected override async Task OnInitializedAsync()
    {
        ToastService.OnChange += UpdateToast;
        Notifications = ToastService.GetToasts();
        AuthenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        var hasDarkTheme = await LocalStorageService.ContainKeyAsync("DarkTheme");
        if (hasDarkTheme)
        {
            var darkTheme = await LocalStorageService.GetItemAsync<bool>("DarkTheme");
            if(darkTheme)
                await ToggleDarkMode();
        }
        else
        {
                await ToggleDarkMode();
        }
        var authStateTask = AuthenticationStateProvider.GetAuthenticationStateAsync();
        OnAuthenticationStateChanged(authStateTask);
    }

    protected bool IsNonNavPage()
    {
        string[] nonNavPages = ["/", "/Account/Register", "/Account/Login", "/404"];
        return nonNavPages.Any(page => NavManager.Uri.EndsWith(page));
    }

    protected void UpdateToast()
    {
        Notifications = ToastService.GetToasts();
        StateHasChanged();
    }

    public void Dispose()
    {
        ToastService.OnChange -= UpdateToast;
        AuthenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
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

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        Task.Run(async () =>
        {
            if (AuthenticationState == null) return;
            var authState = await AuthenticationState;
            var user = authState?.User;
            if (user?.Identity is not null && user.Identity.IsAuthenticated)
            {
                var currentUser = await AuthenticateServiceClient.GetUserFromFromAuthState(authState);
                UserState.Current = currentUser;
                HasDob = currentUser?.Dob != null;
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                UserState.Current = null;
                NavManager.NavigateTo("/Account/Login");
                await InvokeAsync(StateHasChanged);
            }
        });
    }
    public bool OpenNav { get; set; }

    public void ToggleNav()
    {
        OpenNav = !OpenNav;
        StateHasChanged();
    }
    
}
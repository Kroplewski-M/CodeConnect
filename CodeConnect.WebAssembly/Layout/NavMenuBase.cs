using ApplicationLayer;
using ApplicationLayer.Classes;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using ClientApplicationLayer.Interfaces;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Layout;

public class NavMenuBase : ComponentBase, IAsyncDisposable
{
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required IFollowingService FollowingService { get; set; }
    [Inject] public required ILocalStorageService LocalStorageService { get; set; }
    [Inject] public required IClientNotificationsService NotificationsService { get; set; }
    [Inject] public required IJSRuntime Js { get; set; }
    [Inject] public required ToastService ToastService { get; set; }
    [Parameter] public bool OpenNav { get; set; }
    [Parameter] public EventCallback<bool> OpenNavChanged { get; set; }
    [CascadingParameter] public required UserState UserState { get; set; }
    
    protected bool LoadingUserDetails { get; set; } = true;
    protected bool Authenticated { get; set; }
    protected int NotificationCount { get; set; } = 0;
    protected HubConnection? HubConnection { get; set; }
    protected int FollowerCount { get; set; } = 0;
    protected int FollowingsCount { get; set; } = 0;
    
    protected override async Task OnInitializedAsync()
    {
        await ConnectToSignalR();
        NotificationsService.OnNotificationCountChanged +=  SetNotificationCount;
        var isMobile = await Js.InvokeAsync<bool>("viewportHelper.isMobile");
        if (!isMobile)
        {
            OpenNav = true;
            await OpenNavChanged.InvokeAsync(OpenNav);
        }
    }
    protected override async Task OnParametersSetAsync()
    {
        if (UserState?.Current != null && Authenticated)
            return;
        if (!string.IsNullOrWhiteSpace(UserState?.Current?.UserName))
        {
            Authenticated = true;
            LoadingUserDetails = true;
            var userFollowersAndFollowing = await FollowingService.GetUserFollowersCount(UserState.Current?.UserName!);
            FollowerCount = userFollowersAndFollowing.FollowersCount;
            FollowingsCount = userFollowersAndFollowing.FollowingCount;
            LoadingUserDetails = false;
            SetNotificationCount();
            StateHasChanged();
        }
    }

    private async Task ConnectToSignalR()
    {
        HubConnection = new HubConnectionBuilder().WithUrl($"{Consts.Base.BaseUrl}{Consts.SignalR.HubName}", options =>
        {
            options.AccessTokenProvider = async () => await LocalStorageService.GetItemAsync<string?>(Consts.Tokens.AuthToken);
        }).Build();
        HubConnection.On(Consts.SignalR.NotificationMethodWatch, () =>
        { 
            NotificationsService.AddNotification();
        });
        await HubConnection.StartAsync();
    }
    private async void SetNotificationCount()
    {
        try
        {
            NotificationCount = await NotificationsService.GetUsersNotificationsCount(UserState?.Current?.Id);
            await InvokeAsync(StateHasChanged);
        }
        catch
        {
            ToastService.PushToast(new Toast("An error occured when fetching notifications.", ToastType.Error));
        }
    }
    protected async Task NavigateAndCloseNav(string url)
    {
        NavigationManager.NavigateTo(url);
        var isMobile = await Js.InvokeAsync<bool>("viewportHelper.isMobile");
        if (isMobile)
        {
            OpenNav = false;
            await OpenNavChanged.InvokeAsync(OpenNav);
        }
    }
    public async ValueTask DisposeAsync()
    { 
        if (HubConnection is not null)
        { 
            await HubConnection.DisposeAsync();
        }

        NotificationsService.OnNotificationCountChanged -= SetNotificationCount;
    }
}
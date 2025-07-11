using ApplicationLayer.Classes;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using ClientApplicationLayer.Interfaces;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace CodeConnect.WebAssembly.Layout;

public class NavMenuBase : ComponentBase, IAsyncDisposable
{
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required IFollowingService FollowingService { get; set; }
    [CascadingParameter] public required UserState UserState { get; set; }
    [Inject] public required ILocalStorageService LocalStorageService { get; set; }
    [Inject] public required IClientNotificationsService NotificationsService { get; set; }
    
    protected bool LoadingUserDetails { get; set; } = true;
    protected bool Authenticated { get; set; }
    protected int NotificationCount { get; set; } = 0;
    protected HubConnection? HubConnection { get; set; }
    protected bool _openNav = false;
    protected int FollowerCount { get; set; } = 0;
    protected int FollowingsCount { get; set; } = 0;
    
    protected override async Task OnInitializedAsync()
    {
        HubConnection = new HubConnectionBuilder().WithUrl($"{Consts.Base.BaseUrl}{Consts.SignalR.HubName}", options =>
        {
            options.AccessTokenProvider = async () => await LocalStorageService.GetItemAsync<string?>(Consts.Tokens.AuthToken);
        }).Build();
        HubConnection.On(Consts.SignalR.NotificationMethodWatch, () =>
        {
            NotificationCount++;
            InvokeAsync(StateHasChanged);
        });
        await HubConnection.StartAsync();

        NotificationCount = await NotificationsService.GetUsersNotificationsCount();
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
            StateHasChanged();
        }
    }

    protected void NavigateAndCloseNav(string url)
    {
        NavigationManager.NavigateTo(url);
        _openNav = false;
    }
    public async ValueTask DisposeAsync()
    { 
        if (HubConnection is not null)
        { 
            await HubConnection.DisposeAsync();
        }
    }
}
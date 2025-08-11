using System.Net.Http.Json;
using ApplicationLayer;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.ExtensionClasses;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClientApplicationLayer.Services;

public class AuthenticateServiceClient(
    HttpClient httpClient,
    ILocalStorageService localStorageService,
    AuthenticationStateProvider authenticationStateProvider,
    IUserService userService,
    NavigationManager navigationManager,ToastService toastService)
    : IAuthenticateServiceClient
{

    public async Task<AuthResponse> CreateUser(RegisterForm registerForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/RegisterUser", registerForm);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (!response.IsSuccessStatusCode || authResponse == null || !authResponse.Flag)
            return new AuthResponse(false, "", "", authResponse?.Message ?? "An error occured please try again later");
        await localStorageService.SetItemAsync(Consts.Tokens.AuthToken, authResponse.Token);
        await localStorageService.SetItemAsync(Consts.Tokens.RefreshToken, authResponse.Token);

        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
        return authResponse;
    }

    public async Task<AuthResponse> LoginUser(LoginForm loginForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/LoginUser", loginForm);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (response.IsSuccessStatusCode)
        {
            if (authResponse != null && authResponse.Flag)
            {
                await localStorageService.SetItemAsync(Consts.Tokens.AuthToken, authResponse.Token);
                await localStorageService.SetItemAsync(Consts.Tokens.RefreshToken, authResponse.RefreshToken);
                ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
                return authResponse;
            }
        }
        return new AuthResponse(false, "","", authResponse?.Message ?? "Error occured during login please try again later");
    }

    private UserDetails? LastFetchedUser { get; set; }
    private readonly SemaphoreSlim _fetchLock = new(1, 1); 
    public async Task<UserDetails?> GetUserFromFromAuthState(AuthenticationState? authState)
    {
        var username = authState.GetUserInfo(Consts.ClaimTypes.UserName);

        await _fetchLock.WaitAsync();
        try
        {
            if (!string.IsNullOrWhiteSpace(LastFetchedUser?.UserName) && username == LastFetchedUser.UserName)
                return LastFetchedUser;
            if(string.IsNullOrWhiteSpace(username))
                return null;
            var user = await userService.GetUserDetails(username);
            LastFetchedUser = user;
            return user;
        }
        finally
        {
            _fetchLock.Release();
        }
    }
    public async Task<AuthResponse> LogoutUser()
    {
        await localStorageService.RemoveItemAsync(Consts.Tokens.AuthToken);
        await localStorageService.RemoveItemAsync(Consts.Tokens.RefreshToken);
        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
        navigationManager.NavigateTo("/");
        toastService.PushToast(new Toast("Logged out successfully",ToastType.Success));
        return new AuthResponse(true, "","", "Logged out successfully");
    }
}

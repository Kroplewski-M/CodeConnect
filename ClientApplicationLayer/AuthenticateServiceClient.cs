using System.Globalization;
using System.Net.Http.Json;
using ApplicationLayer;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.ExtensionClasses;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using DomainLayer.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClientApplicationLayer;

public class AuthenticateServiceClient(
    HttpClient httpClient,
    ILocalStorageService localStorageService,
    AuthenticationStateProvider authenticationStateProvider,
    IUserService userService,
    NavigationManager navigationManager,NotificationsService notificationsService)
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

    public async Task<UserDetails?> GetUserFromFromAuthState(AuthenticationState? authState)
    {
        var username = authState.GetUserInfo(Consts.ClaimTypes.UserName);
        return await userService.GetUserDetails(username);
    }
    public async Task<AuthResponse> LogoutUser()
    {
        await localStorageService.RemoveItemAsync(Consts.Tokens.AuthToken);
        await localStorageService.RemoveItemAsync(Consts.Tokens.RefreshToken);
        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
        navigationManager.NavigateTo("/");
        notificationsService.PushNotification(new Notification("Logged out successfully",NotificationType.Success));
        return new AuthResponse(true, "","", "Logged out successfully");
    }
}

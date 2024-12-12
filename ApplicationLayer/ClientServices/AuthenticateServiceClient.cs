using System.Globalization;
using System.Net.Http.Json;
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ApplicationLayer.ExtensionClasses;
using DomainLayer.Constants;
using DomainLayer.DbEnts;

namespace ApplicationLayer.ClientServices;

public class AuthenticateServiceClient(
    HttpClient httpClient,
    ILocalStorageService localStorageService,
    AuthenticationStateProvider authenticationStateProvider,
    NavigationManager navigationManager,NotificationsService notificationsService)
    : IAuthenticateServiceClient
{
    public event Action? OnChange;
    public void NotifyStateChanged() => OnChange?.Invoke();
    public async Task<AuthResponse> CreateUser(RegisterForm registerForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/RegisterUser", registerForm);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (response.IsSuccessStatusCode)
        {
            if (authResponse != null && authResponse.Flag)
            {
                await localStorageService.SetItemAsync(Constants.Tokens.AuthToken, authResponse.Token);
                await localStorageService.SetItemAsync(Constants.Tokens.RefreshToken, authResponse.Token);

                ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
                NotifyStateChanged();
                return authResponse;
            }
        }
        return new AuthResponse(false, "","", authResponse?.Message ?? "An error occured please try again later");
    }

    public async Task<AuthResponse> LoginUser(LoginForm loginForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/LoginUser", loginForm);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (response.IsSuccessStatusCode)
        {
            if (authResponse != null && authResponse.Flag)
            {
                await localStorageService.SetItemAsync(Constants.Tokens.AuthToken, authResponse.Token);
                await localStorageService.SetItemAsync(Constants.Tokens.RefreshToken, authResponse.RefreshToken);
                ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
                NotifyStateChanged();
                return authResponse;
            }
        }
        return new AuthResponse(false, "","", authResponse?.Message ?? "Error occured during login please try again later");
    }

    public UserDetails GetUserFromFromAuthState(AuthenticationState? authState)
    {
        var dob = authState.GetUserInfo(Constants.ClaimTypes.Dob).Trim() ?? null;
        var createdAt = authState.GetUserInfo(Constants.ClaimTypes.CreatedAt).Trim() ?? null;
        string format = "MM/dd/yyyy";
        var profileImg = authState.GetUserInfo(Constants.ClaimTypes.ProfileImg);
        var backgroundImg = authState.GetUserInfo(Constants.ClaimTypes.BackgroundImg);

        if (string.IsNullOrEmpty(profileImg))
            profileImg = Constants.ProfileDefaults.ProfileImg;
        if (string.IsNullOrEmpty(backgroundImg))
            backgroundImg = Constants.ProfileDefaults.BackgroundImg;

        return new UserDetails(
            FirstName: authState.GetUserInfo(Constants.ClaimTypes.FirstName),
            LastName: authState.GetUserInfo(Constants.ClaimTypes.LastName),
            Email: authState.GetUserInfo(Constants.ClaimTypes.Email),
            UserName: authState.GetUserInfo(Constants.ClaimTypes.UserName),
            ProfileImg: profileImg,
            BackgroundImg: backgroundImg,
            GithubLink: authState.GetUserInfo(Constants.ClaimTypes.GithubLink),
            WebsiteLink: authState.GetUserInfo(Constants.ClaimTypes.WebsiteLink),
            Dob: DateOnly.ParseExact(dob ?? "", format, CultureInfo.InvariantCulture), 
            CreatedAt:DateOnly.ParseExact(createdAt ?? "", format, CultureInfo.InvariantCulture),
            Bio:authState.GetUserInfo(Constants.ClaimTypes.Bio)
            );
    }
    public async Task<AuthResponse> LogoutUser()
    {
        await localStorageService.RemoveItemAsync(Constants.Tokens.AuthToken);
        await localStorageService.RemoveItemAsync(Constants.Tokens.RefreshToken);
        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
        NotifyStateChanged();
        navigationManager.NavigateTo("/");
        notificationsService.PushNotification(new Notification("Logged out successfully",NotificationType.Success));
        return new AuthResponse(true, "","", "Logged out successfully");
    }
}

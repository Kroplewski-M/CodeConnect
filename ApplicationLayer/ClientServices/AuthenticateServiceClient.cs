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
using DomainLayer.Helpers;

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
                await localStorageService.SetItemAsync(Consts.Tokens.AuthToken, authResponse.Token);
                await localStorageService.SetItemAsync(Consts.Tokens.RefreshToken, authResponse.Token);

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
                await localStorageService.SetItemAsync(Consts.Tokens.AuthToken, authResponse.Token);
                await localStorageService.SetItemAsync(Consts.Tokens.RefreshToken, authResponse.RefreshToken);
                ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
                NotifyStateChanged();
                return authResponse;
            }
        }
        return new AuthResponse(false, "","", authResponse?.Message ?? "Error occured during login please try again later");
    }

    public UserDetails GetUserFromFromAuthState(AuthenticationState? authState)
    {
        var dob = authState.GetUserInfo(Consts.ClaimTypes.Dob).Trim() ?? null;
        var createdAt = authState.GetUserInfo(Consts.ClaimTypes.CreatedAt).Trim() ?? null;
        string format = "MM/dd/yyyy";
        var profileImg = authState.GetUserInfo(Consts.ClaimTypes.ProfileImg);
        var backgroundImg = authState.GetUserInfo(Consts.ClaimTypes.BackgroundImg);

        if (string.IsNullOrEmpty(profileImg))
            profileImg = Consts.ProfileDefaults.ProfileImg;
        if (string.IsNullOrEmpty(backgroundImg))
            backgroundImg = Consts.ProfileDefaults.BackgroundImg;

        return new UserDetails(
            FirstName: authState.GetUserInfo(Consts.ClaimTypes.FirstName),
            LastName: authState.GetUserInfo(Consts.ClaimTypes.LastName),
            Email: authState.GetUserInfo(Consts.ClaimTypes.Email),
            UserName: authState.GetUserInfo(Consts.ClaimTypes.UserName),
            ProfileImg: Helpers.GetAzureImgUrl(Consts.ImageType.ProfileImages,profileImg),
            BackgroundImg: Helpers.GetAzureImgUrl(Consts.ImageType.BackgroundImages,backgroundImg),
            GithubLink: authState.GetUserInfo(Consts.ClaimTypes.GithubLink),
            WebsiteLink: authState.GetUserInfo(Consts.ClaimTypes.WebsiteLink),
            Dob: DateOnly.ParseExact(dob ?? "", format, CultureInfo.InvariantCulture), 
            CreatedAt:DateOnly.ParseExact(createdAt ?? "", format, CultureInfo.InvariantCulture),
            Bio:authState.GetUserInfo(Consts.ClaimTypes.Bio)
            );
    }
    public async Task<AuthResponse> LogoutUser()
    {
        await localStorageService.RemoveItemAsync(Consts.Tokens.AuthToken);
        await localStorageService.RemoveItemAsync(Consts.Tokens.RefreshToken);
        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
        NotifyStateChanged();
        navigationManager.NavigateTo("/");
        notificationsService.PushNotification(new Notification("Logged out successfully",NotificationType.Success));
        return new AuthResponse(true, "","", "Logged out successfully");
    }
}

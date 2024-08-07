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
using ClaimTypes = DomainLayer.Constants.ClaimTypes;

namespace ApplicationLayer.ClientServices;

public class AuthenticateServiceClient(
    HttpClient httpClient,
    ILocalStorageService localStorageService,
    AuthenticationStateProvider authenticationStateProvider,
    NavigationManager navigationManager,NotificationsService notificationsService)
    : IAuthenticateServiceClient
{
    public async Task<AuthResponse> CreateUser(RegisterForm registerForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/RegisterUser", registerForm);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (response.IsSuccessStatusCode)
        {
            if (authResponse != null && authResponse.Flag)
            {
                await localStorageService.SetItemAsync(Tokens.AuthToken, authResponse.Token);
                await localStorageService.SetItemAsync(Tokens.RefreshToken, authResponse.RefreshToken);
                ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
                return new AuthResponse(true, authResponse.Token, authResponse.RefreshToken, "Registered successfully");
            }
        }
        return new AuthResponse(false, "", "", authResponse?.Message ?? "An error occured please try again later");
    }

    public async Task<AuthResponse> LoginUser(LoginForm loginForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/LoginUser", loginForm);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (response.IsSuccessStatusCode)
        {
            if (authResponse != null && authResponse.Flag)
            {
                await localStorageService.SetItemAsync(Tokens.AuthToken, authResponse.Token);
                await localStorageService.SetItemAsync(Tokens.RefreshToken, authResponse.RefreshToken);
                ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
                return new AuthResponse(true, authResponse.Token, authResponse.RefreshToken, authResponse.Message);
            }
        }
        return new AuthResponse(false, "", "", authResponse?.Message ?? "Error occured during login please try again later");
    }

    public UserDetails GetUserFromFromAuthState(AuthenticationState? authState)
    {
        var dob = authState.GetUserInfo(ClaimTypes.Dob).Trim() ?? null;
        var createdAt = authState.GetUserInfo(ClaimTypes.CreatedAt).Trim() ?? null;
        string format = "MM/dd/yyyy";
        var profileImg = authState.GetUserInfo(ClaimTypes.ProfileImg);
        var backgroundImg = authState.GetUserInfo(ClaimTypes.BackgroundImg);

        if (string.IsNullOrEmpty(profileImg))
            profileImg = "images/profileImg.jpg";
        if (string.IsNullOrEmpty(backgroundImg))
            backgroundImg = "images/background.jpg";

        return new UserDetails(
            FirstName: authState.GetUserInfo(ClaimTypes.FirstName),
            LastName: authState.GetUserInfo(ClaimTypes.LastName),
            Email: authState.GetUserInfo(ClaimTypes.Email),
            ProfileImg: profileImg,
            BackgroundImg: backgroundImg,
            GithubLink: authState.GetUserInfo(ClaimTypes.GithubLink),
            WebsiteLink: authState.GetUserInfo(ClaimTypes.WebsiteLink),
            Dob: DateOnly.ParseExact(dob ?? "", format, CultureInfo.InvariantCulture), 
            CreatedAt:DateOnly.ParseExact(createdAt ?? "", format, CultureInfo.InvariantCulture),
            Bio:authState.GetUserInfo(ClaimTypes.Bio)
            );
    }
    public async Task<AuthResponse> LogoutUser()
    {
        await localStorageService.RemoveItemAsync(Tokens.AuthToken);
        await localStorageService.RemoveItemAsync(Tokens.RefreshToken);
        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
        navigationManager.NavigateTo("/");
        notificationsService.PushNotification(new Notification("Logged out successfully",NotificationType.Success));
        return new AuthResponse(true, "", "", "Logged out successfully");
    }
    private async Task CheckIfUserIsValid()
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity == null)
            await LogoutUser();
        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
    }
}

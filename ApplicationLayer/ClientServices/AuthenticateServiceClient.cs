using System.Globalization;
using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ApplicationLayer.ExtensionClasses;
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
                await localStorageService.SetItemAsync("AuthToken", authResponse.Token);
                await localStorageService.SetItemAsync("RefreshToken", authResponse.RefreshToken);
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
                await localStorageService.SetItemAsync("AuthToken", authResponse.Token);
                await localStorageService.SetItemAsync("RefreshToken", authResponse.RefreshToken);
                ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
                return new AuthResponse(true, authResponse.Token, authResponse.RefreshToken, authResponse.Message);
            }
        }
        return new AuthResponse(false, "", "", authResponse?.Message ?? "Error occured during login please try again later");
    }

    public UserDetails GetUserFromFromAuthState(AuthenticationState? authState)
    {
        var dob = authState.GetUserInfo("DOB").Trim() ?? null;
        string format = "MM/dd/yyyy";
        var profileImg = authState.GetUserInfo("ProfileImg");
        var backgroundImg = authState.GetUserInfo("BackgroundImg");

        if (string.IsNullOrEmpty(profileImg))
            profileImg = "images/profileImg.jpg";
        if (string.IsNullOrEmpty(backgroundImg))
            backgroundImg = "images/background.jpg";

        return new UserDetails(
            firstName: authState.GetUserInfo("FirstName"),
            lastName: authState.GetUserInfo("LastName"),
            email: authState.GetUserInfo("Email"),
            profileImg: profileImg,
            BackgroundImg: backgroundImg,
            githubLink: authState.GetUserInfo("GithubLink"),
            websiteLink: authState.GetUserInfo("WebsiteLink"),
            DOB: DateOnly.ParseExact(dob ?? "", format, CultureInfo.InvariantCulture), 
            bio:authState.GetUserInfo("Bio")
            );
    }
    public async Task<AuthResponse> LogoutUser()
    {
        await localStorageService.RemoveItemAsync("AuthToken");
        await localStorageService.RemoveItemAsync("RefreshToken");
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

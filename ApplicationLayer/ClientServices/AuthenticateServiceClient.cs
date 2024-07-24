using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ApplicationLayer.ClientServices;

public class AuthenticateServiceClient(HttpClient httpClient,
    ILocalStorageService localStorageService, AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager) : IAuthenticateServiceClient
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
                var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
                var DOB = authState.User.FindFirst(c => c.Type == "DOB")?.Value ?? null; 
                var user = new UserDetails(
                    firstName: authState.User.FindFirst(c => c.Type == "FirstName")?.Value ?? "",
                    lastName: authState.User.FindFirst(c => c.Type == "LastName")?.Value ?? "",
                    email: authState.User.FindFirst(c => c.Type == "ProfileImg")?.Value ?? "",
                    profileImg: authState.User.FindFirst(c => c.Type == "ProfileImg")?.Value ?? "",
                    BackgroundImg: authState.User.FindFirst(c => c.Type == "BackgroundImg")?.Value ?? "",
                    githubLink: authState.User.FindFirst(c => c.Type == "GithubLink")?.Value ?? "",
                    websiteLink:authState.User.FindFirst(c => c.Type == "WebsiteLink")?.Value ?? "",
                    DOB: DateOnly.Parse(DOB), 
                    bio:authState.User.FindFirst(c => c.Type == "Bio")?.Value ?? "");
                _userDetails = user;
                ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
                return new AuthResponse(true, authResponse.Token, authResponse.RefreshToken, authResponse.Message);
            }
        }
        return new AuthResponse(false, "", "", authResponse?.Message ?? "Error occured during login please try again later");
    }

    public async Task<AuthResponse> LogoutUser()
    {
        await localStorageService.RemoveItemAsync("AuthToken");
        await localStorageService.RemoveItemAsync("RefreshToken");
        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
        navigationManager.NavigateTo("/");
        return new AuthResponse(true, "", "", "Logged out successfully");
    }

    private UserDetails _userDetails { get; set; } = new UserDetails("","","","","","","", null, "");
    public UserDetails GetUserDetails() => _userDetails;

    public async void CheckIfUserIsValid()
    {
        var user = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (user.User.Identity == null)
            await LogoutUser();
    }
}

using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ApplicationLayer.ClientServices;

public class AuthenticateServiceClient(
    HttpClient httpClient,
    ILocalStorageService localStorageService,
    AuthenticationStateProvider authenticationStateProvider,
    NavigationManager navigationManager)
    : IAuthenticateServiceClient
{
    private UserDetails _userDetails { get; set; } = new UserDetails("","","","","","","", null, "");
    private Timer? _timer { get; set; } = null;

    public event Action? OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();
    public async Task InitializeAsync()
    {
        await CheckIfUserIsValid();
        _timer = new Timer(async _ => await CheckIfUserIsValid(), null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
    }


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
                await CheckIfUserIsValid();
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
                await CheckIfUserIsValid();
                
                return new AuthResponse(true, authResponse.Token, authResponse.RefreshToken, authResponse.Message);
            }
        }
        return new AuthResponse(false, "", "", authResponse?.Message ?? "Error occured during login please try again later");
    }

    private UserDetails GetUserFromFromAuthState(AuthenticationState? authState)
    {
        var DOB = authState.User.FindFirst(c => c.Type == "DOB")?.Value ?? null;
        return new UserDetails(
            firstName: authState.User.FindFirst(c => c.Type == "FirstName")?.Value ?? "",
        lastName: authState.User.FindFirst(c => c.Type == "LastName")?.Value ?? "",
        email: authState.User.FindFirst(c => c.Type == "ProfileImg")?.Value ?? "",
        profileImg: authState.User.FindFirst(c => c.Type == "ProfileImg")?.Value ?? "images/profileImg.jpg",
        BackgroundImg: authState.User.FindFirst(c => c.Type == "BackgroundImg")?.Value ?? "images/background/jpg",
        githubLink: authState.User.FindFirst(c => c.Type == "GithubLink")?.Value ?? "",
        websiteLink:authState.User.FindFirst(c => c.Type == "WebsiteLink")?.Value ?? "",
        DOB: DateOnly.MaxValue, 
        bio:authState.User.FindFirst(c => c.Type == "Bio")?.Value ?? "");
    }
    public async Task<AuthResponse> LogoutUser()
    {
        await localStorageService.RemoveItemAsync("AuthToken");
        await localStorageService.RemoveItemAsync("RefreshToken");
        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
        navigationManager.NavigateTo("/");
        return new AuthResponse(true, "", "", "Logged out successfully");
    }

    public UserDetails GetUserDetails() => _userDetails;

    private async Task CheckIfUserIsValid()
    {
        Console.WriteLine("checking if user is valid");
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity == null)
            await LogoutUser();
        _userDetails = GetUserFromFromAuthState(authState);
        NotifyStateChanged();
        Console.WriteLine("User is valid");
        Console.WriteLine("Name: " + authState.User.FindFirst(c => c.Type == "FirstName")?.Value);
    }
}

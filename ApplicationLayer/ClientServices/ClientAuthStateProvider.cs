using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace ApplicationLayer.ClientServices;

public class ClientAuthStateProvider(HttpClient httpClient,
    ILocalStorageService localStorageService, NavigationManager navigationManager) : AuthenticationStateProvider, IAuthenticateServiceClient
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await localStorageService.GetItemAsync<string>(Consts.Tokens.AuthToken);
            var isValid = await IsTokenValid(token);
            if (isValid)
            {
                return CreateAuthenticationStateFromToken(token);
            }
            var refresh = await localStorageService.GetItemAsync<string>(Consts.Tokens.RefreshToken);
            var isValidRefresh = await IsTokenValid(refresh);
            return isValidRefresh ?  CreateAuthenticationStateFromToken(refresh) : new AuthenticationState(new ClaimsPrincipal());
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }
    }
    private async Task<bool> IsTokenValid(string? token)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/ValidateToken", token);
        return response.IsSuccessStatusCode;
    }
    private AuthenticationState CreateAuthenticationStateFromToken(string? token)
    {
        var claimsPrincipal = new ClaimsPrincipal();
        if (!string.IsNullOrWhiteSpace(token))
            claimsPrincipal = DecodeToken(token);
        var authState = new AuthenticationState(claimsPrincipal);
        var user = Task.FromResult(authState);
        NotifyAuthenticationStateChanged(user);
        if(navigationManager.Uri.EndsWith("/"))
            navigationManager.NavigateTo("/MyFeed");
        return authState;
    }
    private ClaimsPrincipal DecodeToken(string? token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);

        var claims = new List<Claim>();
        foreach (var claim in jwtSecurityToken.Claims)
        {
            claims.Add(new Claim(claim.Type, claim.Value));
        }

        var identity = new ClaimsIdentity(claims, Consts.Tokens.AuthType);
        return new ClaimsPrincipal(identity);
    }
    public event Action? OnChange;
    public void NotifyStateChanged() => OnChange?.Invoke();
    public async Task<AuthResponse> CreateUser(RegisterForm registerForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/RegisterUser", registerForm);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (!response.IsSuccessStatusCode || authResponse == null || !authResponse.Flag)
            return new AuthResponse(false, "", "", authResponse?.Message ?? "An error occured please try again later");
        await localStorageService.SetItemAsync(Consts.Tokens.AuthToken, authResponse.Token);
        await localStorageService.SetItemAsync(Consts.Tokens.RefreshToken, authResponse.RefreshToken);
        
        StateChange(authResponse.Token);
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
                
                StateChange(authResponse.Token);
                return authResponse;
            }
        }
        return new AuthResponse(false, "","", authResponse?.Message ?? "Error occured during login please try again later");
    }

    private void StateChange(string? token = "")
    {
        CreateAuthenticationStateFromToken(token);
        NotifyStateChanged();
    }
    public async Task<UserDetails?> GetUserFromAuthState(AuthenticationState? authState)
    {
        return await httpClient.GetFromJsonAsync<UserDetails?>("/api/User/GetUserDetails");
    }
    public async Task<AuthResponse> LogoutUser()
    {
        await localStorageService.RemoveItemAsync(Consts.Tokens.AuthToken);
        await localStorageService.RemoveItemAsync(Consts.Tokens.RefreshToken);
        StateChange();
        navigationManager.NavigateTo("/");
        return new AuthResponse(true, "","", "Logged out successfully");
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using Blazored.LocalStorage;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace ApplicationLayer.ClientServices;

public class ClientAuthStateProvider(HttpClient httpClient,
    ILocalStorageService localStorageService,NavigationManager navigationManager) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await localStorageService.GetItemAsync<string>("AuthToken");
            if (await IsTokenValid(token ?? ""))
            {
                return CreateAuthenticationStateFromToken(token ?? "");
            }

            var refreshToken = await localStorageService.GetItemAsync<string>("RefreshToken");
            if (!await IsTokenValid(refreshToken ?? ""))
            {
                return new AuthenticationState(new ClaimsPrincipal());
            }

            var newToken = await RefreshToken(refreshToken ?? "");
            if (newToken != null)
            {
                await localStorageService.SetItemAsync("AuthToken", newToken);
                return CreateAuthenticationStateFromToken(newToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        return new AuthenticationState(new ClaimsPrincipal());
    }

    private async Task<bool> IsTokenValid(string token)
    {
        var response = await httpClient.PostAsJsonAsync("/api/ValidateToken", token);
        return response.IsSuccessStatusCode;
    }
    private AuthenticationState CreateAuthenticationStateFromToken(string token)
    {
        var authState = new AuthenticationState(new ClaimsPrincipal(DecodeToken(token)));
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
        return authState;
    }
    private async Task<AuthenticationState>  RedirectToLoginPage()
    {
        await RemoveAllAuth();
        navigationManager.NavigateTo("/Account/Login");
        var authState = new AuthenticationState(new ClaimsPrincipal());
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
        return authState;
    }
    private async Task RemoveAllAuth()
    {
        await localStorageService.RemoveItemAsync("RefreshToken");
        await localStorageService.RemoveItemAsync("AuthToken");
    }

    private async Task<string?> RefreshToken(string refreshToken)
    {
        var responseMessage = await httpClient.PostAsJsonAsync("/api/RefreshToken", refreshToken);
        if (responseMessage.IsSuccessStatusCode)
        {
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
            return response?.Key;
        }
        return null;
    }

    private ClaimsPrincipal DecodeToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);

        var claims = new List<Claim>();
        foreach (var claim in jwtSecurityToken.Claims)
        {
            claims.Add(new Claim(claim.Type, claim.Value));
        }
    
        var identity = new ClaimsIdentity(claims, "Jwt");
        return new ClaimsPrincipal(identity);
    }
    public async Task<ServiceResponse> LogIn(LoginForm loginForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/LoginUser", loginForm);
        var authResponse = response.Content.ReadFromJsonAsync<AuthResponse>().Result;
        if (response.IsSuccessStatusCode)
        {
            if (authResponse != null && authResponse.Flag)
            {
                await localStorageService.SetItemAsync("AuthToken", authResponse.Token);
                await localStorageService.SetItemAsync("RefreshToken", authResponse.RefreshToken);
                NotifyAuthenticationStateChanged(Task.FromResult(CreateAuthenticationStateFromToken(authResponse.Token ?? "")));
                return new ServiceResponse(true, authResponse?.Message.FirstOrDefault() ?? "success");
            }
        }
        return new ServiceResponse(false, authResponse?.Message.FirstOrDefault() ?? "Error occured");
    }

    public async Task<ServiceResponse> Register(RegisterForm registerForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/RegiserUser", registerForm);
        var authResponse = response.Content.ReadFromJsonAsync<AuthResponse>().Result;
        if (response.IsSuccessStatusCode)
        {
            if (authResponse != null && authResponse.Flag)
            {
                await localStorageService.SetItemAsync("AuthToken", authResponse.Token);
                await localStorageService.SetItemAsync("RefreshToken", authResponse.RefreshToken);
                NotifyAuthenticationStateChanged(
                    Task.FromResult(CreateAuthenticationStateFromToken(authResponse.Token ?? "")));
                return new ServiceResponse(true, "Registered successfully");
            }
        }
        return new ServiceResponse(false, "Error while registering");
    }

    public async Task<AuthenticationState> LogOut()
    {
        return await RedirectToLoginPage();
    }
}
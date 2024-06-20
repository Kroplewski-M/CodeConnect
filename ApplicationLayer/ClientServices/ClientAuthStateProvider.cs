using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using Blazored.LocalStorage;
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
                return await RedirectToLoginPage();
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
        return await RedirectToLoginPage();
    }

    private async Task<bool> IsTokenValid(string token)
    {
        var response = await httpClient.PostAsJsonAsync("/api/ValidateToken", token);
        return response.IsSuccessStatusCode;
    }
    private AuthenticationState CreateAuthenticationStateFromToken(string token)
    {
        return new AuthenticationState(new ClaimsPrincipal(DecodeToken(token)));
    }
    private async Task<AuthenticationState>  RedirectToLoginPage()
    {
        await RemoveAllAuth();
        navigationManager.NavigateTo("/Account/Login");
        return new AuthenticationState(new ClaimsPrincipal());
    }
    private async Task SetToken(string name,string token)
    {
        await localStorageService.SetItemAsync<string>(name, token);
        var authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }
    
    public async Task RemoveAllAuth()
    {
        await localStorageService.RemoveItemAsync("RefreshToken");
        await localStorageService.RemoveItemAsync("AuthToken");
        var authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
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
    public async Task LogIn()
    {
        
    }

    public async Task Register()
    {
        
    }
}
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
            var isAuthedFromToken = !string.IsNullOrEmpty(token) && await IsTokenValid(token);
            if (isAuthedFromToken)
            {
                return CreateAuthenticationStateFromToken(token ?? "");
            }

            var refreshToken = await localStorageService.GetItemAsync<string>("RefreshToken");
            isAuthedFromToken = !string.IsNullOrEmpty(refreshToken) && await IsTokenValid(refreshToken);
            if (!isAuthedFromToken)
            {
                return new AuthenticationState(new ClaimsPrincipal());
            }

            var newToken = !string.IsNullOrEmpty(refreshToken) ? await RefreshToken(refreshToken) : null;
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
    public void NotifyStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    private async Task<bool> IsTokenValid(string token)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/ValidateToken", token);
        return response.IsSuccessStatusCode;
    }
    private AuthenticationState CreateAuthenticationStateFromToken(string token)
    {
        var authState = new AuthenticationState(new ClaimsPrincipal(DecodeToken(token)));
        var test = Task.FromResult(authState);
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
        return authState;
    }
    private async Task<string?> RefreshToken(string refreshToken)
    {
        var responseMessage = await httpClient.PostAsJsonAsync("/api/Authentication/RefreshToken", refreshToken);
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
}
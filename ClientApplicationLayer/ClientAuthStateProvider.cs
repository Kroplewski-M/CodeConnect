using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using Blazored.LocalStorage;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace ApplicationLayer.ClientServices;

public class ClientAuthStateProvider(HttpClient httpClient,
    ILocalStorageService localStorageService, NavigationManager navigationManager) : AuthenticationStateProvider
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
    public void NotifyStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    private async Task<bool> IsTokenValid(string? token)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/ValidateToken", token);
        return response.IsSuccessStatusCode;
    }
    private AuthenticationState CreateAuthenticationStateFromToken(string? token)
    {
        var authState = new AuthenticationState(new ClaimsPrincipal(DecodeToken(token)));
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
}

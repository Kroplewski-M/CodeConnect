using System.Security.Claims;
using ApplicationLayer.APIServices;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
namespace ApplicationLayer.ClientServices;

public class ClientAuthStateProvider(HttpClient httpClient,
    ILocalStorageService localStorageService,NavigationManager navigationManager,TokenService tokenService) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await localStorageService.GetItemAsync<string>("AuthToken");
        var refreshToken = await localStorageService.GetItemAsync<string>("RefreshToken");
        var user = tokenService.ValidateToken(token ?? "");
        
        if (user == null)
        {
            var verifyRefresh = tokenService.ValidateToken(refreshToken ?? "");
            if (verifyRefresh == null)
            {
                navigationManager.NavigateTo("/Account/Login");
                return new AuthenticationState(new ClaimsPrincipal());
            }
            else
            {
                //refresh token
                //return user
                return new AuthenticationState(new ClaimsPrincipal());
            }
        }
        return new AuthenticationState(new ClaimsPrincipal());
    }
    public async void SetToken(string token)
    {
        await localStorageService.SetItemAsync<string>("AuthToken", token);
    }

    public async void RemoveAllAuth()
    {
        await localStorageService.RemoveItemAsync("RefreshToken");
        await localStorageService.RemoveItemAsync("AuthToken");
    }
}
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
        //Send api request to verify token
        var user = "";
        //update to check if claims are empty
        if (user == null)
        {
            var refreshToken = await localStorageService.GetItemAsync<string>("RefreshToken");
            //Send api request to verify token
            var verifyRefresh = "";
            //update to check if claims are empty
            if (verifyRefresh == null)
            {
                navigationManager.NavigateTo("/Account/Login");
                return new AuthenticationState(new ClaimsPrincipal());
            }
            else
            {
                //refresh token
                //return claimsPrincipal
                return new AuthenticationState(new ClaimsPrincipal());
            }
        }
        return new AuthenticationState(new ClaimsPrincipal());
    }
    public async Task SetToken(string token)
    {
        await localStorageService.SetItemAsync<string>("AuthToken", token);
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

    public async Task LogIn()
    {
        
    }

    public async Task Register()
    {
        
    }
}
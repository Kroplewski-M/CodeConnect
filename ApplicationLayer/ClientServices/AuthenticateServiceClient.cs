using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ApplicationLayer.ClientServices;

public class AuthenticateServiceClient(HttpClient httpClient,
    ILocalStorageService localStorageService,NavigationManager navigationManager, AuthenticationStateProvider authenticationStateProvider) : IAuthenticateService
{
    public async Task<AuthResponse> CreateUser(RegisterForm registerForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/RegisterUser", registerForm);
        var authResponse =  await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (response.IsSuccessStatusCode)
        {
            if (authResponse != null && authResponse.Flag)
            {
                await localStorageService.SetItemAsync("AuthToken", authResponse.Token);
                await localStorageService.SetItemAsync("RefreshToken", authResponse.RefreshToken);
                ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
                return new AuthResponse(true, authResponse.Token ,authResponse.RefreshToken,"Registered successfully");
            }
        }
        return new AuthResponse(false,"","" ,"Error while registering");
    }

    public async Task<AuthResponse> LoginUser(LoginForm loginForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/LoginUser", loginForm);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (response.IsSuccessStatusCode)
        {
            if (authResponse != null && authResponse.Flag)
            {
                await localStorageService.SetItemAsync("AuthToken", authResponse.Token);
                await localStorageService.SetItemAsync("RefreshToken", authResponse.RefreshToken);
                await localStorageService.SetItemAsync("RefreshToken", authResponse.RefreshToken);
                ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
                return new AuthResponse(true, authResponse.Token ,authResponse.RefreshToken, authResponse.Message);
            }
        }
        return new AuthResponse(false,"","", authResponse?.Message);
    }
}
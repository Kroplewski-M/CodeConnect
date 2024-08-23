using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Constants;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ApplicationLayer.ClientServices;

public class UserServiceClient(HttpClient httpClient,ILocalStorageService localStorageService, 
    AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager) : IUserService
{
    public async Task<ServiceResponse> UpdateUserDetails(EditProfileForm editProfileForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/EditUserDetails", editProfileForm);
        var newToken = await response.Content.ReadFromJsonAsync<TokenResponse>();
        await localStorageService.SetItemAsync(Constants.Tokens.AuthToken, newToken?.Key);
        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
        return new ServiceResponse(true, "Updated Details Successfully");
    }

    public async Task<UserDetails?> GetUserDetails(string username)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/GetUserDetails", username);
        if(response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<UserDetails>();
        navigationManager.NavigateTo("UserNotFound");
        return null;
    }
}
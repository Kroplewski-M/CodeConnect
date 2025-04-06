using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Constants;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components.Authorization;

namespace ApplicationLayer.ClientServices;

public class UserImageServiceClient(HttpClient httpClient,ILocalStorageService localStorageService,
    AuthenticationStateProvider authenticationStateProvider, ClientAuthStateProvider authenticateServiceClient): IUserImageService
{
    public async Task<ServiceResponse> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/UpdateUserImage", updateUserImageRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if (string.IsNullOrEmpty(result?.Key))
            return new ServiceResponse(false, "An error occured, please try again later.");
        
        await localStorageService.SetItemAsync(Consts.Tokens.AuthToken, result.Key);
        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
        authenticateServiceClient.NotifyStateChanged();
        return new ServiceResponse(true, "User image updated");
    }
}
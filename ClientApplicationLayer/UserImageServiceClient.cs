using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Constants;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClientApplicationLayer;

public class UserImageServiceClient(HttpClient httpClient,ILocalStorageService localStorageService,
    AuthenticationStateProvider authenticationStateProvider): IUserImageService
{
    public async Task<ServiceResponse> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest,string? userId = null)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/UpdateUserImage", updateUserImageRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if (!string.IsNullOrWhiteSpace(result?.Key))
        {
            await localStorageService.SetItemAsync(Consts.Tokens.AuthToken, result.Key);
            ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
            return new ServiceResponse(true, "User image updated");
        }
        return new ServiceResponse(false, "An error occured, please try again later.");
    }
}
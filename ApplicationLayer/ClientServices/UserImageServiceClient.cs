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
    AuthenticationStateProvider authenticationStateProvider): IUserImageService
{
    public async Task<ServiceResponse> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest)
    {
        using var content = new MultipartFormDataContent();
        
        var fileContent = new StreamContent(updateUserImageRequest.ImageStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(updateUserImageRequest.ContentType);
        content.Add(fileContent, "image", updateUserImageRequest.FileName);
        content.Add(new StringContent(((int)updateUserImageRequest.TypeOfImage).ToString()), "typeOfImage");
        var response = await httpClient.PostAsync("/api/User/UpdateUserImage", content);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (result != null && result.Flag)
        {
            var newToken = await response.Content.ReadFromJsonAsync<TokenResponse>();
            await localStorageService.SetItemAsync(Constants.Tokens.AuthToken, newToken?.Key);
            ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
            return new ServiceResponse(true, "Updated Details Successfully");
        }
        return new ServiceResponse(false, "An error occured, please try again later.");
    }
}
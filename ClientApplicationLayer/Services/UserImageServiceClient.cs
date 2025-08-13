using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using ClientApplicationLayer.Interfaces;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClientApplicationLayer.Services;

public class UserImageServiceClient(HttpClient httpClient,ILocalStorageService localStorageService,
    ICachedAuth cachedAuth): IUserImageService
{
    public async Task<ServiceResponse> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest,string? userId = null)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/UpdateUserImage", updateUserImageRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        cachedAuth.ClearCacheAndNotify(); 
        if(result?.Flag == true)
            return result;
        return new ServiceResponse(false, "An error occured while updating the image");
    }
}
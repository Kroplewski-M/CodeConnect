using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Constants;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ApplicationLayer.ClientServices;

public class UserImageServiceClient(HttpClient httpClient): IUserImageService
{
    public async Task<ServiceResponse> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/UpdateUserImage", updateUserImageRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        if (result?.Flag ?? false)
        {
            return result;
        }
        return new ServiceResponse(false, "An error occured, please try again later.");
    }
}
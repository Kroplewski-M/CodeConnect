using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using ClientApplicationLayer.Interfaces;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClientApplicationLayer.Services;

public class UserServiceClient(HttpClient httpClient,ILocalStorageService localStorageService, 
   ICachedAuth cachedAuth, NavigationManager navigationManager) : IUserService
{
    public async Task<ServiceResponse> UpdateUserDetails(EditProfileForm editProfileForm, string? userId = null)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/EditUserDetails", editProfileForm);
        var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        if (result?.Flag == true)
        {
            cachedAuth.ClearCacheAndNotify(); 
            return new ServiceResponse(true, "Updated Details Successfully");
        }
        return new ServiceResponse(false, "invalid response when updating details");
    }

    public async Task<UserDetails?> GetUserDetails(string username)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/GetUserDetails", username);
        if(response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<UserDetails?>();
        navigationManager.NavigateTo("UserNotFound");
        return null;
    }

    public async Task<UserInterestsDto> GetUserInterests(string username)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/GetUserInterests", username);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UserInterestsDto>();
            if(result != null)
                return result;
        }
        return new UserInterestsDto(false, "failed to fetch interests", null);
    }

    public async Task<ServiceResponse> UpdateUserInterests(UpdateTechInterestsDto interests)
    {
        var response = await httpClient.PutAsJsonAsync("api/User/UpdateUserInterests",interests);
        var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        if (result is { Flag: true })
        {
            cachedAuth.ClearCacheAndNotify(); 
            return result;
        }
        return new ServiceResponse(false, "An error occured while updating the interests");
    }

    public async Task<List<TechInterestsDto>> GetAllInterests()
    {
        var response = await httpClient.GetFromJsonAsync<List<TechInterestsDto>>("api/User/GetAllInterests");
        return response ?? new List<TechInterestsDto>();
    }
}
using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Constants;
using DomainLayer.DbEnts;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ApplicationLayer.ClientServices;

public class UserServiceClient(HttpClient httpClient,ILocalStorageService localStorageService, NavigationManager navigationManager, IAuthenticateServiceClient authenticateServiceClient) : IUserService
{
    public async Task<ServiceResponse> UpdateUserDetails(EditProfileForm editProfileForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/EditUserDetails", editProfileForm);
        var newTokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if (!string.IsNullOrWhiteSpace(newTokens?.Key))
        {
            await localStorageService.SetItemAsync(Consts.Tokens.AuthToken, newTokens?.Key);
            authenticateServiceClient.NotifyStateChanged();
            return new ServiceResponse(true, "Updated Details Successfully");
        }
        return new ServiceResponse(false, "invalid response when updating details");
    }

    public async Task<UserDetails?> GetUserDetails(string? userId)
    {
        var response = await httpClient.GetFromJsonAsync<UserDetails?>("/api/User/GetUserDetails");
        if(response != null)
            return response;
        navigationManager.NavigateTo("UserNotFound");
        return null;
    }

    public async Task<UserInterestsDto> GetUserInterests(string? userId = null)
    {
        var response = await httpClient.GetFromJsonAsync<UserInterestsDto>("/api/User/GetUserInterests");
        if (response != null)
            return response;
        return new UserInterestsDto(false, "failed to fetch interests", null);
    }

    public async Task<ServiceResponse> UpdateUserInterests(UpdateTechInterestsDto interests)
    {
        var response = await httpClient.PutAsJsonAsync("api/User/UpdateUserInterests",interests);
        var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        if (result != null)
            return result;
        return new ServiceResponse(false, "An error occured while updating the interests");
    }

    public async Task<List<TechInterestsDto>> GetAllInterests()
    {
        var response = await httpClient.GetFromJsonAsync<List<TechInterestsDto>>("api/User/GetAllInterests");
        return response ?? new List<TechInterestsDto>();
    }
}
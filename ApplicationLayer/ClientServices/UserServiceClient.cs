using System.Net.Http.Json;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities;

namespace ApplicationLayer.ClientServices;

public class UserServiceClient(HttpClient httpClient) : IUserService
{
    public async Task UpdateUserDetails(EditProfileForm editProfileForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/EditUserDetails", editProfileForm);   
        
    }
}
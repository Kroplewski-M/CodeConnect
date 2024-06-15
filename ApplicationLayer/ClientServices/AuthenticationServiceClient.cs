using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using System.Net.Http.Json;
namespace ApplicationLayer.ClientServices;

public class AuthenticationServiceClient(HttpClient httpClient) : IAuthenticateService
{
    public async Task<TokenResponse> CreateUser(RegisterFormViewModel registerForm)
    {
        try
        {
            httpClient = new HttpClient();
            var result = await httpClient.PostAsJsonAsync(
                "https://localhost:7124/api/Authentication/CreateUser"
                , registerForm);
            if (result.IsSuccessStatusCode)
            {
                var tokenResponse = await result.Content.ReadFromJsonAsync<TokenResponse>();
                return tokenResponse;
            }
        }
        catch (HttpRequestException err)
        {
            Console.WriteLine(err);
        }
        return null;
    }

    public Task<TokenResponse> LoginUser()
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse> LogoutUser()
    {
        throw new NotImplementedException();
    }
}
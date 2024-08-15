using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Constants;
using Newtonsoft.Json;

namespace CodeConnect.WebAssembly.DelegatingHandler;


public class AuthHandler(ILocalStorageService localStorageService) : System.Net.Http.DelegatingHandler
{
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await localStorageService.GetItemAsync<string>(Tokens.AuthToken);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        var response = await base.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var refreshRequest = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7124/api/Authentication/RefreshToken");
            refreshRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var tokenResponse = await base.SendAsync(refreshRequest, cancellationToken);
            var refreshToken = await tokenResponse.Content.ReadAsStringAsync();
            await localStorageService.SetItemAsync(Tokens.AuthToken,refreshToken); 
        }
        return response;
    }
    
}
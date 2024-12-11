using System.IdentityModel.Tokens.Jwt;
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
        var token = await localStorageService.GetItemAsync<string>(Constants.Tokens.AuthToken, cancellationToken);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(Constants.Tokens.ApiAuthTokenName, token);
        }
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshToken = await localStorageService.GetItemAsync<string>(Constants.Tokens.RefreshToken, cancellationToken);
            var refreshRequest = new HttpRequestMessage(HttpMethod.Get, $"{Constants.Base.BaseUrl}/api/Authentication/RefreshToken");
            refreshRequest.Headers.Authorization = new AuthenticationHeaderValue(Constants.Tokens.ApiAuthTokenName, refreshToken);
            var tokenResponse = await base.SendAsync(refreshRequest, cancellationToken);
            var newToken = await tokenResponse.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
            if (newToken?.Flag == true)
            {
                await localStorageService.SetItemAsync(Constants.Tokens.AuthToken,newToken.Token,cancellationToken);
                await localStorageService.SetItemAsync(Constants.Tokens.RefreshToken,newToken.RefreshToken,cancellationToken);
                request.Headers.Authorization = new AuthenticationHeaderValue(Constants.Tokens.ApiAuthTokenName, newToken.Token);
                var retry = await base.SendAsync(request, cancellationToken);
                return retry;
            }
        }
        return response;
    }
    
}
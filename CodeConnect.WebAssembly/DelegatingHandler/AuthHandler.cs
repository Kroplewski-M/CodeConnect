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
        var token = await localStorageService.GetItemAsync<string>(Consts.Tokens.AuthToken, cancellationToken);
        var hasToken = !string.IsNullOrEmpty(token);
        if (hasToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(Consts.Tokens.ApiAuthTokenName, token);
        }
        else if(request.RequestUri?.AbsolutePath != Consts.Base.RegisterEndpoint && request.RequestUri?.AbsolutePath != Consts.Base.LoginEndpoint)
        {
            await RefreshTokenAndRetry(request, cancellationToken);
        }
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized && hasToken)
        {
            await RefreshTokenAndRetry(request, cancellationToken);
        }
        return response;
    }

    private async Task<HttpResponseMessage> RefreshTokenAndRetry(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var refreshToken = await localStorageService.GetItemAsync<string>(Consts.Tokens.RefreshToken, cancellationToken);
        var refreshRequest = new HttpRequestMessage(HttpMethod.Get, $"{Consts.Base.BaseUrl}/api/Authentication/RefreshToken");
        refreshRequest.Headers.Authorization = new AuthenticationHeaderValue(Consts.Tokens.ApiAuthTokenName, refreshToken);
        var tokenResponse = await base.SendAsync(refreshRequest, cancellationToken);
        var newToken = await tokenResponse.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
        if (newToken?.Flag == true)
        {
            await localStorageService.SetItemAsync(Consts.Tokens.AuthToken,newToken.Token,cancellationToken);
            await localStorageService.SetItemAsync(Consts.Tokens.RefreshToken,newToken.RefreshToken,cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue(Consts.Tokens.ApiAuthTokenName, newToken.Token);
            var retry = await base.SendAsync(request, cancellationToken);
            return retry;
        }
        return new HttpResponseMessage(HttpStatusCode.Unauthorized);
    }
}
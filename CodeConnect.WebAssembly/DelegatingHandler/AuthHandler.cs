using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApplicationLayer;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace CodeConnect.WebAssembly.DelegatingHandler;


public class AuthHandler(ILocalStorageService localStorageService, NavigationManager navigationManager) : System.Net.Http.DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var deviceId = await localStorageService.GetItemAsync<string>(Consts.Headers.DeviceId, cancellationToken);
        if (deviceId == null)
        {
            deviceId = Guid.NewGuid().ToString();
            await localStorageService.SetItemAsync(Consts.Headers.DeviceId, deviceId, cancellationToken);
        }
        request.Headers.Add(Consts.Headers.DeviceId, deviceId);
        
        var token = await localStorageService.GetItemAsync<string>(Consts.Tokens.AuthToken, cancellationToken);
        var hasToken = !string.IsNullOrEmpty(token);
        HttpResponseMessage? response = null;
        if (request.RequestUri == null)
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        if (hasToken || Consts.AuthEndpoints.AuthUrls.Contains(request.RequestUri.AbsolutePath))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(Consts.Tokens.ApiAuthTokenName, token);
            response = await base.SendAsync(request, cancellationToken);
        }

        if (response?.StatusCode == HttpStatusCode.Forbidden)
        {
            navigationManager.NavigateTo("/Unauthorized");
            return response;       
        }
        if (!hasToken || response?.StatusCode == HttpStatusCode.Unauthorized)
        {
            return await RefreshTokenAndRetry(request, cancellationToken);
        }
        return response!;
    }

    private async Task<HttpResponseMessage> RefreshTokenAndRetry(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var refreshToken = await localStorageService.GetItemAsync<string>(Consts.Tokens.RefreshToken, cancellationToken);
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, $"{Consts.Base.BaseUrl}/api/Authentication/RefreshToken");
        refreshRequest.Headers.Authorization = new AuthenticationHeaderValue(Consts.Tokens.ApiAuthTokenName, refreshToken);
        var deviceId = await localStorageService.GetItemAsync<string>(Consts.Headers.DeviceId, cancellationToken);
        if(!string.IsNullOrWhiteSpace(deviceId))
            refreshRequest.Headers.TryAddWithoutValidation(Consts.Headers.DeviceId, deviceId);
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
        await localStorageService.RemoveItemAsync(Consts.Tokens.RefreshToken, cancellationToken);
        await localStorageService.RemoveItemAsync(Consts.Tokens.TokenType, cancellationToken);
        return new HttpResponseMessage(HttpStatusCode.Unauthorized);
    }

}
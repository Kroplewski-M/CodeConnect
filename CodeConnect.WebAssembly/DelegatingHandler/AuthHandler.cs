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
        var token = await localStorageService.GetItemAsync<string>(Constants.Tokens.AuthToken);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(Constants.Tokens.ApiAuthTokenName, token);
        }
        var response = await base.SendAsync(request, cancellationToken);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
        if (expClaim != null && long.TryParse(expClaim.Value, out var expUnix))
        {
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            var timeRemaining = expirationTime - DateTime.UtcNow;
            if (timeRemaining.TotalMinutes < 10)
            {
                var refreshRequest = new HttpRequestMessage(HttpMethod.Get, $"{Constants.Base.BaseUrl}/api/Authentication/RefreshToken");
                refreshRequest.Headers.Authorization = new AuthenticationHeaderValue(Constants.Tokens.ApiAuthTokenName, token);
                var tokenResponse = await base.SendAsync(refreshRequest, cancellationToken);
                var refreshToken = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
                await localStorageService.SetItemAsync(Constants.Tokens.AuthToken,refreshToken,cancellationToken); 
            }
        }
        return response;
    }
    
}
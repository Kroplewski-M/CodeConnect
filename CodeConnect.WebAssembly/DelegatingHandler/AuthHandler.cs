using System.Net;
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
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        var response = await base.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            
        }
        return response;
    }
    
}
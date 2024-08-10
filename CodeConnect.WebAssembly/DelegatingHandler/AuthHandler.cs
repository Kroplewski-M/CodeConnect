using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using DomainLayer.Constants;

namespace CodeConnect.WebAssembly.DelegatingHandler;


public class AuthHandler(ILocalStorageService localStorageService) : System.Net.Http.DelegatingHandler
{
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        var token = await localStorageService.GetItemAsync<string>(Tokens.AuthToken);
        Console.WriteLine("This is form the handler token: " + token);
        return response;
    }
}
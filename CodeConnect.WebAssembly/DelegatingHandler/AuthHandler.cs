using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;

namespace CodeConnect.WebAssembly.DelegatingHandler;


public class AuthHandler
    : System.Net.Http.DelegatingHandler
{
    
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = base.SendAsync(request, cancellationToken);
        Console.WriteLine("This is form the handler");
        return response;
    }
}
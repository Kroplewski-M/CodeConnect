using ApplicationLayer;
using ApplicationLayer.APIServices;
using ApplicationLayer.ClientServices;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CodeConnect.WebAssembly;
using CodeConnect.WebAssembly.DelegatingHandler;
using DomainLayer.Constants;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddOptions();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddTransient<AuthHandler>();
builder.Services.AddHttpClient("DefaultClient",
    client =>
    {
        client.BaseAddress = new Uri(Constants.Base.BaseUrl);
    }).AddHttpMessageHandler<AuthHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("DefaultClient"));
builder.Services.AddScoped<AuthenticationStateProvider,ClientAuthStateProvider>();
builder.Services.AddScoped<IAuthenticateServiceClient, AuthenticateServiceClient>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IUserService, UserServiceClient>();
builder.Services.AddTransient<IUserImageService,UserImageServiceClient>();
builder.Services.AddSingleton<NotificationsService>();
await builder.Build().RunAsync();


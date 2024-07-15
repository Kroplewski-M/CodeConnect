using ApplicationLayer.APIServices;
using ApplicationLayer.ClientServices;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CodeConnect.WebAssembly;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddOptions();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7124") });
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider,ClientAuthStateProvider>();
builder.Services.AddScoped<IAuthenticateService, AuthenticateServiceClient>();
builder.Services.AddSingleton<NotificationsService>();
await builder.Build().RunAsync();


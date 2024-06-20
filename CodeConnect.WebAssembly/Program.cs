using ApplicationLayer.APIServices;
using ApplicationLayer.ClientServices;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CodeConnect.WebAssembly;
using Microsoft.AspNetCore.Components.Authorization;
using TokenService = ApplicationLayer.ClientServices.TokenService;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSingleton<AuthenticationStateProvider,TokenService>();
await builder.Build().RunAsync();
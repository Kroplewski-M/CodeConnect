using ApplicationLayer;
using ApplicationLayer.Interfaces;
using Blazored.LocalStorage;
using ClientApplicationLayer;
using ClientApplicationLayer.Interfaces;
using ClientApplicationLayer.Services;
using CodeConnect.WebAssembly;
using CodeConnect.WebAssembly.DelegatingHandler;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddOptions();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddTransient<AuthHandler>();
builder
    .Services.AddHttpClient(
        "DefaultClient",
        client =>
        {
            client.BaseAddress = new Uri(Consts.Base.BaseUrl);
        }
    )
    .AddHttpMessageHandler<AuthHandler>();
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("DefaultClient")
);
builder.Services.AddScoped<AuthenticationStateProvider, ClientAuthStateProvider>();
builder.Services.AddScoped<IAuthenticateServiceClient, AuthenticateServiceClient>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IUserService, UserServiceClient>();
builder.Services.AddTransient<IUserImageService, UserImageServiceClient>();
builder.Services.AddSingleton<ToastService>();
builder.Services.AddSingleton<ImageConvertorServiceClient>();
builder.Services.AddScoped<IFollowingService, FollowingServiceClient>();
builder.Services.AddScoped<IPostService, PostServiceClient>();
builder.Services.AddScoped<MarkdigServiceClient>();
builder.Services.AddScoped<IClientNotificationsService, NotificationsServiceClient>();
await builder.Build().RunAsync();

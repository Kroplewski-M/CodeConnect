using ApplicationLayer.ClientServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CodeConnect.WebAssembly.Components.Feed;

public class CreatePostBase : ComponentBase
{
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }
    [Inject]
    IAuthenticateServiceClient AuthenticateServiceClient { get; set; } = null!;
    protected UserDetails? UserDetails = null;

    protected string PostContent { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationState;
        var user = authState?.User;

        if (user?.Identity is not null && user.Identity.IsAuthenticated)
        {
            UserDetails = AuthenticateServiceClient.GetUserFromFromAuthState(authState);
            await InvokeAsync(StateHasChanged);
        }
    }

    protected void HandleValidSubmit()
    {
        Console.WriteLine("HandleValidSubmit");
    }
}
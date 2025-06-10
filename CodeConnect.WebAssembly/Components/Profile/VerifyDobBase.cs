using ApplicationLayer.Classes;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class VerifyDobBase : ComponentBase
{
    [CascadingParameter] public required UserState UserState { get; set; }
    [Inject] public required IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    protected bool Loading { get; set; } = false;
    protected DateOnly? Dob { get; set; }
    protected async Task SaveDob()
    {
        Console.WriteLine(Dob);
        await Task.CompletedTask;
    }

    protected async Task Logout()
    {
        await AuthenticateServiceClient.LogoutUser();
    }
}
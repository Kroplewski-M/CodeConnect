using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class FollowUserBase : ComponentBase
{
    [Inject]
    public required IUserService UserService { get; set; } 
    protected bool Following { get; set; } = false;
    protected bool IsHovering { get; set; } = false;
    [Parameter]
    public required string CurrentUsername { get; set; }
    [Parameter]
    public required string FollowUsername { get; set; }

    protected void ToggleFollow()
    {
        Console.WriteLine($"Current user: {CurrentUsername}");
        Console.WriteLine($"Target user: {FollowUsername}");

        var request = new FollowRequestDto(CurrentUsername, FollowUsername);
        if (Following)
        {
            UserService.FollowUser(request);
        }
        else
        {
            UserService.UnfollowUser(request);
        }
        Following = !Following;
        StateHasChanged();
    }
}
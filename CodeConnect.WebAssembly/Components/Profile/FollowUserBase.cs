using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class FollowUserBase : ComponentBase
{
    protected bool Following { get; set; } = false;
    protected bool IsHovering { get; set; } = false;
    protected void ToggleFollow()
    {
        Following = !Following;
        StateHasChanged();
    }
}
using ApplicationLayer.Classes;
using Microsoft.AspNetCore.Components;
namespace CodeConnect.WebAssembly.Pages;

public class HomeBase : ComponentBase
{
    [Inject] public required NavigationManager NavigationManager { get; set; } = null!;
    [CascadingParameter] public required UserState UserState { get; set; }

    protected override void OnInitialized()
    {
        if(UserState?.Current != null)
            NavigationManager.NavigateTo("/MyFeed");
    }
}
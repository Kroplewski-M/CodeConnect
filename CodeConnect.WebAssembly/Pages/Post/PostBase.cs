using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages.Post;

public class PostBase : ComponentBase
{
    [Parameter] public string Id { get; set; } = "";
    protected override async Task OnParametersSetAsync()
    {
        await Task.Delay(1);
        Console.WriteLine("PostBase");
    }
}
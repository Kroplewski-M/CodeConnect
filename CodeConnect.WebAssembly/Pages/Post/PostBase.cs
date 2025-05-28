using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages.Post;

public class PostBase : ComponentBase
{
    [Parameter] public required int Id { get; set; }
    [Inject] public required IPostService PostService { get; set; }
    protected override async Task OnParametersSetAsync()
    {
        var post = await PostService.GetPostById(Id);
    }
}
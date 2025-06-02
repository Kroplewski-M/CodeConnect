using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages.Post;

public class PostBase : ComponentBase
{
    [Parameter] public required Guid Id { get; set; }
    [Inject] public required IPostService PostService { get; set; }
    protected bool Loading { get; set; } = true;
    protected PostBasicDto? Post { get; set; }
    protected override async Task OnParametersSetAsync()
    {
        Loading = true;
        Post = await PostService.GetPostById(Id);
        Loading = false;
    }
}
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Pages.Post;

public class PostBase : ComponentBase
{
    [Parameter] public required Guid Id { get; set; }
    [Inject] public required IPostService PostService { get; set; }
    [Inject] public required IJSRuntime Js { get; set; }
    protected bool Loading { get; set; } = true;
    protected PostBasicDto? Post { get; set; }
    protected override async Task OnParametersSetAsync()
    {
        SetLoading(true);
        Post = await PostService.GetPostById(Id);
        SetLoading(false);
        await Js.InvokeVoidAsync("highlightCodeBlocks",Id.ToString());
        
    }
    private void SetLoading(bool isLoading)
    {
        Loading = isLoading;
        StateHasChanged();
    }

}
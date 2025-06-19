using ApplicationLayer.Classes;
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
    [CascadingParameter] public required UserState UserState { get; set; }
    protected bool Loading { get; set; } = true;
    protected PostBasicDto? Post { get; set; }
    protected bool IsUserLiking { get; set; }
    protected int LikeCount { get; set; }
    protected override async Task OnParametersSetAsync()
    {
        SetLoading(true);
        Post = await PostService.GetPostById(Id);
        LikeCount = Post?.LikeCount ?? 0;
        IsUserLiking = await PostService.IsUserLikingPost(Id, UserState.Current?.UserName ?? "");
        SetLoading(false);
        await Js.InvokeVoidAsync("highlightCodeBlocks",Id.ToString());
    }
    private void SetLoading(bool isLoading)
    {
        Loading = isLoading;
        StateHasChanged();
    }
    private bool LoadingLike { get; set; }
    protected async Task ToggleLike()
    {
        if (LoadingLike)
            return;
        LoadingLike = true;
        await PostService.ToggleLikePost(new LikePostDto(Id, UserState.Current?.UserName ?? ""));
        IsUserLiking = !IsUserLiking;
        if(IsUserLiking)
            LikeCount++;
        else
            LikeCount--;
        LoadingLike = false;
        StateHasChanged();
    }
}
using ApplicationLayer;
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
    [Inject] public required ToastService ToastService { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [CascadingParameter] public required UserState UserState { get; set; }
    protected bool Loading { get; set; } = true;
    protected PostBasicDto? Post { get; set; }
    protected bool IsUserLiking { get; set; }
    protected int LikeCount { get; set; }
    protected bool IsPostCreator {get;set;}
    protected override async Task OnInitializedAsync()
    {
        SetLoading(true);
        Post = await PostService.GetPostById(Id);
        LikeCount = Post?.LikeCount ?? 0;
        IsUserLiking = await PostService.IsUserLikingPost(Id, UserState.Current?.UserName ?? "");
        SetLoading(false);
        IsPostCreator = UserState.Current?.UserName == Post?.CreatedByUsername;
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
        var res = await PostService.ToggleLikePost(new LikePostDto(Id));
        if (res.Flag)
        {
            IsUserLiking = !IsUserLiking;
            if(IsUserLiking)
                LikeCount++;
            else
                LikeCount--;
        }
        else
        {
            ToastService.PushToast(new Toast(res.Message, ToastType.Error));
        }
        LoadingLike = false;
        StateHasChanged();
    }
    protected bool ConfirmDeletePost { get; set; }
    protected void ToggleDeletPost()
    {
        ConfirmDeletePost = !ConfirmDeletePost;
    }
    protected async Task DeletePost()
    {
        var result = await PostService.DeletePost(Id);
        if (result.Flag)
        {
           ToastService.PushToast(new Toast(result.Message, ToastType.Success)); 
            NavigationManager.NavigateTo("/MyFeed");
        }
        else
        {
            ToastService.PushToast(new Toast(result.Message, ToastType.Error));
        }
    }
}
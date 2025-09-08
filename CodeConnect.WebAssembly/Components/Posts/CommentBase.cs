using ApplicationLayer;
using ApplicationLayer.Classes;
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Components.Posts;

public class CommentBase : ComponentBase
{
    
    [Parameter] public EventCallback<Guid> OnCommentDeletion { get; set; }
    [Parameter] public required CommentDto UserComment { get; set; }
    [CascadingParameter] public required UserState UserState { get; set; }
    [CascadingParameter] public Guid? HighlightCommentId { get; set; }
    [Inject] public required IJSRuntime Js  { get; set; }
    [Inject] public required IPostService PostService { get; set; }
    [Inject] public required ToastService ToastService { get; set; }
    protected readonly string _id = Guid.NewGuid().ToString();
    protected int LikeCount { get; set; }
    protected bool LikesComment { get; set; }

    protected override void OnInitialized()
    {
        LikeCount = UserComment.LikeCount;
        LikesComment = UserComment.CurrentUserLikes;
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Js.InvokeVoidAsync("highlightCodeBlocks",_id);
            StateHasChanged();
        }
    }

    protected async Task LikeComment()
    {
        var res = await PostService.ToggleLikeComment(UserComment.Id);
        
        Console.WriteLine($"res : {res.Flag}");
        if (res.Flag)
        {
            if (LikesComment)
            {
                LikesComment = false;
                LikeCount--;
            }
            else
            {
                LikesComment = true;
                LikeCount++;
            }
            StateHasChanged(); 
        }
    }
    protected bool Loading = false;
    protected async Task DeleteComment()
    {
        Loading = true;
        ToastService.PushToast(new Toast("Deleting comment", ToastType.Info));
        StateHasChanged();
        var res = await PostService.DeleteComment(UserComment.Id);
        if (res.Flag)
        {
            await OnCommentDeletion.InvokeAsync(UserComment.Id);
            ToastService.PushToast(new Toast(res.Message, ToastType.Success));
        }
        else
        {
            ToastService.PushToast(new Toast(res.Message, ToastType.Error));
        }
        ShowDeleteComment = false;
        Loading = false;
    }
    protected bool ShowDeleteComment = false;
    public void ToggleShowDeleteComment()
    {
        ShowDeleteComment = !ShowDeleteComment;
        StateHasChanged();
    }
}
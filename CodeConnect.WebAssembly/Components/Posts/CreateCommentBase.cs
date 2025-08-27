using ApplicationLayer;
using ApplicationLayer.Classes;
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.Interfaces;
using ClientApplicationLayer.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Components.Posts;

public class CreateCommentBase : ComponentBase
{
    [Parameter] public EventCallback<CommentDto> OnCommentCreated { get; set; }
    [CascadingParameter] public required Guid PostId { get; set; }
    [Inject] public required IPostService PostService { get; set; } 
    [Inject] public required ToastService ToastService { get; set; }
    [Inject] public required MarkdigServiceClient MarkdigService { get; set; }
    protected string Comment { get; set; } = "";
    protected bool ShowPreview { get; set; } = false;
    protected bool Loading { get; set; } = false;
    protected void TogglePreview()
    {
        ShowPreview = !ShowPreview;
    }

    protected async Task CreateComment()
    {
        if (string.IsNullOrWhiteSpace(Comment)) return;
        Loading = true;
        ToastService.PushToast(new Toast("Creating Comment", ToastType.Info));
        var result = await PostService.UpsertPostComment(PostId,commentId:null, MarkdigService.ConvertToHtmlOnlyCode(Comment));
        ToastService.PushToast(new Toast(result.Message, result.Flag ? ToastType.Success : ToastType.Error));
        if (result.Flag)
        {
            await OnCommentCreated.InvokeAsync(result.Comment);
        }
        Loading = false;
        Comment = "";
        StateHasChanged();
    }
}
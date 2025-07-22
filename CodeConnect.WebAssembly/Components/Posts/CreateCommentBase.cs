using ApplicationLayer;
using ApplicationLayer.Classes;
using ApplicationLayer.Interfaces;
using ClientApplicationLayer.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Components.Posts;

public class CreateCommentBase : ComponentBase
{
    [CascadingParameter] public required Guid PostId { get; set; }
    [Inject] public required IPostService PostService { get; set; } 
    [Inject] public required ToastService ToastService { get; set; }

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
        var result = await PostService.UpsertPostComment(PostId,commentId:null, Comment);
        ToastService.PushToast(new Toast(result.Message, result.Flag ? ToastType.Success : ToastType.Error));
        Loading = false;
        Comment = "";
        StateHasChanged();
    }
}
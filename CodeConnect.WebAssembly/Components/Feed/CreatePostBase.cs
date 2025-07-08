using ApplicationLayer;
using ApplicationLayer.Classes;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Images;
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using ClientApplicationLayer;
using ClientApplicationLayer.Services;
using DomainLayer.Constants;
using DomainLayer.Helpers;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web.Infrastructure;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Components.Feed;

public class CreatePostBase : ComponentBase
{
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationState { get; set; }
    [Inject] IAuthenticateServiceClient AuthenticateServiceClient { get; set; } = null!;
    [CascadingParameter] public required UserState UserState { get; set; }
    [Inject] public required IJSRuntime Js { get; set; }
    [Inject] public ImageConvertorServiceClient ImageConvertor { get; set; } = null!;
    [Inject] public IPostService PostService { get; set; } = null!;  
    [Inject] public required ToastService ToastService { get; set; }
    [Inject] public required MarkdigServiceClient MarkdigService { get; set; }
    protected string PostContent { get; set; } = string.Empty;
    protected readonly string InputId = "uploadPostImg";
    protected bool LoadingImages = false;
    protected List<Base64Dto> Base64Images = new List<Base64Dto>();
    protected async Task HandleFileSelection(InputFileChangeEventArgs e)
    {
        LoadingImages = true;
        Base64Images.Clear();
        if (e.GetMultipleFiles().Count() > 5)
        {
            ToastService.PushToast(new Toast("You may only select upto 5 images", ToastType.Error));
            LoadingImages = false;
            StateHasChanged();
            return;
        }
        if (e.GetMultipleFiles().Any(x => x.Size > Consts.Base.UploadMaxFileSize))
        {
            ToastService.PushToast(new Toast($"Max file size is {Helpers.BytesToMegabytes(Consts.Base.UploadMaxFileSize)}MB.", ToastType.Error));
            LoadingImages = false;
            StateHasChanged();
            return;
        }
        foreach (var image in e.GetMultipleFiles().ToList())
        {
            Base64Images.Add(new Base64Dto(await ImageConvertor.ImageToBase64(image), Path.GetExtension(image.Name)));
        }
        LoadingImages = false;
        StateHasChanged();
    }
    protected void RemoveFileSelected(int index)
    {
        Base64Images.RemoveAt(index);
        StateHasChanged();
    }
    protected bool Loading { get; set; } = false;
    protected async Task HandleValidSubmit()
    {
        if (string.IsNullOrWhiteSpace(PostContent))
            return;
        var post = new CreatePostDto(MarkdigService.ConvertToHtmlOnlyCode(PostContent), Base64Images);
        var postValidator = new CreatePostDtoValidator();
        var validate = await postValidator.ValidateAsync(post);
        if (!validate.IsValid)
        {
            ToastService.PushToast(new Toast("Error creating post", ToastType.Error));
            return;
        }
        Loading = true;
        ToastService.PushToast(new Toast("Creating Post", ToastType.Info));
        var postResponse = await PostService.CreatePost(post);
        if (postResponse.Flag)
        {
            ToastService.PushToast(new Toast(postResponse.Message, ToastType.Success));
            Base64Images.Clear();
            PostContent = string.Empty;
            StateHasChanged();
        }
        else
            ToastService.PushToast(new Toast(postResponse.Message, ToastType.Error));
        Loading = false;
    }
    protected bool ShowPreview = false;
    protected void ToggleShowPreview()
    {
        ShowPreview = !ShowPreview;
        StateHasChanged();
    }
}
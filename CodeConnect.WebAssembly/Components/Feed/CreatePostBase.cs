using ApplicationLayer;
using ApplicationLayer.ClientServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Images;
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
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
    protected UserDetails? UserDetails = null;
    [Inject] public required IJSRuntime Js { get; set; }
    [Inject] public ImageConvertorServiceClient ImageConvertor { get; set; } = null!;
    [Inject] public IPostService PostService { get; set; } = null!;  
    [Inject] public required NotificationsService NotificationsService { get; set; }
    [Inject] public required MarkdigServiceClient MarkdigService { get; set; }
    protected string PostContent { get; set; } = string.Empty;
    protected readonly string InputId = "uploadPostImg";
    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState != null)
        {
            var authState = await AuthenticationState;
            var user = authState?.User;

            if (user?.Identity is not null && user.Identity.IsAuthenticated)
            {
                UserDetails = AuthenticateServiceClient.GetUserFromFromAuthState(authState);
                //await InvokeAsync(StateHasChanged);
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Js.InvokeVoidAsync("postSizeOnBlur","post");
        }
        if (_shouldHighlight)
        {
            _shouldHighlight = false;
            await Js.InvokeVoidAsync("highlightCodeBlocks");
        }
    }
    protected bool LoadingImages = false;
    protected List<Base64Dto> Base64Images = new List<Base64Dto>();
    protected async Task HandleFileSelection(InputFileChangeEventArgs e)
    {
        LoadingImages = true;
        Base64Images.Clear();
        if (e.GetMultipleFiles().Count() > 5)
        {
            NotificationsService.PushNotification(new Notification("You may only select upto 5 images", NotificationType.Error));
            LoadingImages = false;
            StateHasChanged();
            return;
        }
        if (e.GetMultipleFiles().Any(x => x.Size > Consts.Base.UploadMaxFileSize))
        {
            NotificationsService.PushNotification(new Notification($"Max file size is {Helpers.BytesToMegabytes(Consts.Base.UploadMaxFileSize)}MB.", NotificationType.Error));
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
        var post = new CreatePostDto(MarkdigService.ConvertToHtmlOnlyCode(PostContent), Base64Images, UserDetails?.UserName ?? "");
        var postValidator = new CreatePostDtoValidator();
        var validate = await postValidator.ValidateAsync(post);
        if (!validate.IsValid)
        {
            NotificationsService.PushNotification(new Notification("Error creating post", NotificationType.Error));
            return;
        }
        Loading = true;
        NotificationsService.PushNotification(new Notification("Creating Post", NotificationType.Info));
        var postResponse = await PostService.CreatePost(post);
        if (postResponse.Flag)
        {
            NotificationsService.PushNotification(new Notification(postResponse.Message, NotificationType.Success));
            Base64Images.Clear();
            PostContent = string.Empty;
            PreviewText = string.Empty;
            StateHasChanged();
        }
        else
            NotificationsService.PushNotification(new Notification(postResponse.Message, NotificationType.Error));
        Loading = false;
    }
    protected string PreviewText = string.Empty;
    private bool _shouldHighlight;
    protected async Task PreviewMarkdown(string content)
    {
        PreviewText = MarkdigService.ConvertToHtmlOnlyCode(content);
        _shouldHighlight = true;
        StateHasChanged();
        await Js.InvokeVoidAsync("highlightCodeBlocks");
    }
    protected bool ShowPreview = false;
    protected readonly string PreviewTextId = Guid.NewGuid().ToString();
    protected async Task ToggleShowPreview()
    {
        ShowPreview = !ShowPreview;
        StateHasChanged();
        await Js.InvokeVoidAsync("highlightCodeBlocks",PreviewTextId);
    }
    protected async Task OnInput()
    {
        var content = await Js.InvokeAsync<string>("getValueById", "post");

        await PreviewMarkdown(content);
        await Js.InvokeVoidAsync("autoResizeTextAreaAndContainer","post");
        await Js.InvokeVoidAsync("highlightCodeBlocks",PreviewTextId);
    }
}
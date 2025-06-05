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
    /// <summary>
    /// Initializes the component by retrieving and setting the authenticated user's details if available.
    /// </summary>
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

    /// <summary>
    /// Invoked after the component has rendered; initializes post input resizing on first render and triggers code block highlighting if required.
    /// </summary>
    /// <param name="firstRender">Indicates whether this is the first time the component is rendered.</param>
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
    /// <summary>
    /// Submits a new post with optional images after validating the content and notifies the user of the result.
    /// </summary>
    protected async Task HandleValidSubmit()
    {
        if (string.IsNullOrWhiteSpace(PostContent))
            return;
        var post = new CreatePostDto(PostContent, Base64Images, UserDetails?.UserName ?? "");
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
            StateHasChanged();
        }
        else
            NotificationsService.PushNotification(new Notification(postResponse.Message, NotificationType.Error));
        Loading = false;
    }
    protected string PreviewText = string.Empty;
    private bool _shouldHighlight;
    /// <summary>
    /// Converts the provided Markdown content to HTML code blocks for preview and triggers syntax highlighting.
    /// </summary>
    /// <param name="content">The Markdown content to preview.</param>
    /// <remarks>
    /// Updates the preview text with rendered HTML code blocks and invokes JavaScript to highlight syntax.
    /// </remarks>
    protected async Task PreviewMarkdown(string content)
    {
        PreviewText = MarkdigService.ConvertToHtmlOnlyCode(content);
        _shouldHighlight = true;
        StateHasChanged();
        await Js.InvokeVoidAsync("highlightCodeBlocks");
    }
    protected bool ShowPreview = false;
    /// <summary>
    /// Toggles the visibility of the Markdown preview and triggers syntax highlighting for code blocks.
    /// </summary>
    protected async Task ToggleShowPreview()
    {
        ShowPreview = !ShowPreview;
        await Js.InvokeVoidAsync("highlightCodeBlocks");
        StateHasChanged();
    }
    /// <summary>
    /// Handles input changes by updating the Markdown preview and resizing the post input area.
    /// </summary>
    protected async Task OnInput()
    {
        var content = await Js.InvokeAsync<string>("getValueById", "post");

        await PreviewMarkdown(content);
        await Js.InvokeVoidAsync("autoResizeTextAreaAndContainer","post");
    }
}
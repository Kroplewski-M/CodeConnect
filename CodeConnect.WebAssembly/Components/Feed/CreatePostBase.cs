using ApplicationLayer;
using ApplicationLayer.ClientServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Components.Feed;

public class CreatePostBase : ComponentBase
{
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }
    [Inject]
    IAuthenticateServiceClient AuthenticateServiceClient { get; set; } = null!;
    protected UserDetails? UserDetails = null;
    [Inject]
    public required IJSRuntime Js { get; set; }
    [Inject]
    public ImageConvertorServiceClient ImageConvertor { get; set; } = null!;
    [Inject]
    public IPostService PostService { get; set; } = null!;  
    [Inject]
    public required NotificationsService NotificationsService { get; set; }
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
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Js.InvokeVoidAsync("postSizeOnBlur","post");
        }
    }
    protected bool LoadingImages = false;
    protected List<Base64Dto> Base64Images = new List<Base64Dto>();
    protected async Task HandleFileSelection(InputFileChangeEventArgs e)
    {
        LoadingImages = true;
        Base64Images.Clear();
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
}
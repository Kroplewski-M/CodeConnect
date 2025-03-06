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
    public IJSRuntime Js { get; set; }
    [Inject]
    public ImageConvertorServiceClient ImageConvertor { get; set; } = null!;
    [Inject]
    public IPostService PostService { get; set; } = null!;  
    protected string PostContent { get; set; } = string.Empty;
    protected readonly string InputId = "uploadPostImg";
    protected readonly string ImagePreviewId = "uploadedPostImgPreview";
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationState;
        var user = authState?.User;

        if (user?.Identity is not null && user.Identity.IsAuthenticated)
        {
            UserDetails = AuthenticateServiceClient.GetUserFromFromAuthState(authState);
            await InvokeAsync(StateHasChanged);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Js.InvokeVoidAsync("postSizeOnBlur","post");
        }
    }
    protected bool loadingImages = false;
    protected List<string> Base64Images = new List<string>();
    protected async Task HandleFileSelection(InputFileChangeEventArgs e)
    {
        loadingImages = true;
        Base64Images.Clear();
        foreach (var image in e.GetMultipleFiles().ToList())
        {
            Base64Images.Add(await ImageConvertor.ImageToBase64(image));
        }
        loadingImages = false;
        StateHasChanged();
    }
    protected void RemoveFileSelected(int index)
    {
        Base64Images.RemoveAt(index);
        StateHasChanged();
    }
    protected async Task HandleValidSubmit()
    {
        var post = new PostDTO(PostContent, Base64Images);
        await PostService.CreatePost(post);
    }
}
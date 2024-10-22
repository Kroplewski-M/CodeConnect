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
        await Js.InvokeVoidAsync("PreviewImg",InputId,ImagePreviewId);
        if (firstRender)
        {
            await Js.InvokeVoidAsync("postSizeOnBlur","post");
        }
    }
    protected bool LoadedImg = false;
    protected IBrowserFile? SelectedImg { get; set; } = null;
    protected void HandleFileSelection(InputFileChangeEventArgs e)
    {
        SelectedImg = e.GetMultipleFiles().FirstOrDefault();
        LoadedImg = true;
    }
    protected void HandleValidSubmit()
    {
        Console.WriteLine("HandleValidSubmit");
    }
}
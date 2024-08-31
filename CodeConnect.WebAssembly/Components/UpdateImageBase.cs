using System.Reflection.Metadata;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Components;

public class UpdateImageBase : ComponentBase
{
    [Inject]
    public IJSRuntime Js { get; set; }

    [Inject] public IUserImageService UserImageService { get; set; }

    [Parameter]
    public Constants.ImageTypeOfUpdate UpdateOfImageType { get; set; }
    [Parameter]
    public EventCallback Cancel { get; set; }

    protected bool LoadedImg { get; set; } = false;
    protected IBrowserFile? SelectedImg { get; set; } = null;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
            await Js.InvokeVoidAsync("PreviewImg",SelectedImg);
    }
    protected void HandleFileSelection(InputFileChangeEventArgs e)
    {
        SelectedImg = e.GetMultipleFiles().FirstOrDefault();
        LoadedImg = true;
    }

    protected async Task SaveImg()
    {
        if (SelectedImg != null)
        {
            var maxAllowedSize = 10 * 1024 * 1024; //10MB
            Console.WriteLine("Type: " + UpdateOfImageType);
            var imageStream = SelectedImg.OpenReadStream(maxAllowedSize);
            var updateUserImageRequest = new UpdateUserImageRequest
            {
                TypeOfImage = UpdateOfImageType,
                ImageStream = imageStream,
                ContentType = SelectedImg.ContentType,
                FileName = SelectedImg.Name
            };

            await UserImageService.UpdateUserImage(updateUserImageRequest);
        }
    }
}
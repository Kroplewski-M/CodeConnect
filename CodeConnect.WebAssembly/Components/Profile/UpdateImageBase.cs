using System.Reflection.Metadata;
using ApplicationLayer;
using ApplicationLayer.Classes;
using ApplicationLayer.ExtensionClasses;
using ApplicationLayer.Interfaces;
using ClientApplicationLayer;
using ClientApplicationLayer.Services;
using DomainLayer.Constants;
using DomainLayer.Entities;
using DomainLayer.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Components.Profile;

public class UpdateImageBase : ComponentBase
{
    [Inject] public required IUserImageService UserImageService { get; set; }
    [Inject] public required ToastService ToastService { get; set; }
    [Inject] public required ImageConvertorServiceClient ImageConvertor { get; set; }
    [Inject] public required IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    [Parameter] public Consts.ImageType UpdateOfImageType { get; set; }
    [Parameter] public EventCallback Cancel { get; set; }
    [CascadingParameter] public required UserState UserState { get; set; }
    protected bool Loading { get; set; } = false;
    public UpdateUserImageRequest SelectedImg { get; set; } = new UpdateUserImageRequest();
    protected async Task HandleFileSelection(InputFileChangeEventArgs e)
    {
        var img = e.GetMultipleFiles().FirstOrDefault();
        if (img == null)
            return;
        if (img.Size > Consts.Base.UploadMaxFileSize)
        {
            ToastService.PushToast(new ApplicationLayer.Toast($"Max file size is {Helpers.BytesToMegabytes(Consts.Base.UploadMaxFileSize)}MB.", ToastType.Error));
            return;
        }
        Loading = true;
        SelectedImg.ImgBase64 = await ImageConvertor.ImageToBase64(img);
        SelectedImg.FileName = img.Name;
        SelectedImg.TypeOfImage = UpdateOfImageType;
        Loading = false;
    }
    protected bool LoadingUpdate { get; set; } = false;
    protected async Task SaveImg()
    {
        if (!string.IsNullOrWhiteSpace(SelectedImg.ImgBase64))
        {
            try
            {
                LoadingUpdate = true;
                ToastService.PushToast(new ApplicationLayer.Toast("Updating please wait...", ToastType.Info));
                
                var result = await UserImageService.UpdateUserImage(SelectedImg);
                ToastService.PushToast(result.Flag
                    ? new ApplicationLayer.Toast(result.Message, ToastType.Success)
                    : new ApplicationLayer.Toast(result.Message, ToastType.Error));
            }
            catch
            {
                ToastService.PushToast(new ApplicationLayer.Toast("An error occured please try again later.", ToastType.Error));
            }
            finally
            {
                LoadingUpdate = false;
                await Cancel.InvokeAsync(null);
                StateHasChanged();
            }
            
        }
    }
}
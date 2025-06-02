using System.Reflection.Metadata;
using ApplicationLayer;
using ApplicationLayer.ClientServices;
using ApplicationLayer.ExtensionClasses;
using ApplicationLayer.Interfaces;
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
    [Inject] public required NotificationsService NotificationsService { get; set; }
    [Inject] public required ImageConvertorServiceClient ImageConvertor { get; set; }
    [Inject] public required IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    [Parameter] public Consts.ImageType UpdateOfImageType { get; set; }
    [Parameter] public EventCallback Cancel { get; set; }

    protected bool Loading { get; set; } = false;
    public UpdateUserImageRequest SelectedImg { get; set; } = new UpdateUserImageRequest();
    protected async Task HandleFileSelection(InputFileChangeEventArgs e)
    {
        var img = e.GetMultipleFiles().FirstOrDefault();
        if (img == null)
            return;
        if (img.Size > Consts.Base.UploadMaxFileSize)
        {
            NotificationsService.PushNotification(new ApplicationLayer.Notification($"Max file size is {Helpers.BytesToMegabytes(Consts.Base.UploadMaxFileSize)}MB.", NotificationType.Error));
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
                NotificationsService.PushNotification(new ApplicationLayer.Notification("Updating please wait...", NotificationType.Info));
                SelectedImg.Username = await AuthenticateServiceClient.GetUsersUsername()!;
                
                var result = await UserImageService.UpdateUserImage(SelectedImg);
                NotificationsService.PushNotification(result.Flag
                    ? new ApplicationLayer.Notification(result.Message, NotificationType.Success)
                    : new ApplicationLayer.Notification(result.Message, NotificationType.Error));
            }
            catch
            {
                NotificationsService.PushNotification(new ApplicationLayer.Notification("An error occured please try again later.", NotificationType.Error));
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
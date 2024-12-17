using System.Reflection.Metadata;
using ApplicationLayer;
using ApplicationLayer.ClientServices;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Components.Profile;

public class UpdateImageBase : ComponentBase
{
    [Inject] public required IUserImageService UserImageService { get; set; }
    [Inject] public required NotificationsService NotificationsService { get; set; }
    [Inject] public required ImageConvertorServiceClient ImageConvertor { get; set; }
    [Parameter]
    public Consts.ImageTypeOfUpdate UpdateOfImageType { get; set; }
    [Parameter]
    public EventCallback Cancel { get; set; }

    protected bool loading { get; set; } = false;
    protected bool DisableImg { get; set; } = false;
    protected UpdateUserImageRequest SelectedImg { get; set; } = new UpdateUserImageRequest();
    protected string ImagePreview = string.Empty;
    protected async Task HandleFileSelection(InputFileChangeEventArgs e)
    {
        var img = e.GetMultipleFiles().FirstOrDefault();
        if (img == null)
            return;
        loading = true;
        SelectedImg.ImageStream = ImageConvertor.ImageToStream(img);
        SelectedImg.ContentType = img.ContentType;
        SelectedImg.FileName = img.Name;
        SelectedImg.TypeOfImage = UpdateOfImageType;

        ImagePreview = await ImageConvertor.ImageToBase64(img);
        loading = false;
    }

    protected async Task SaveImg()
    {
        if (SelectedImg != null)
        {
            try
            {
                DisableImg = true;
                NotificationsService.PushNotification(new ApplicationLayer.Notification("Updating please wait...", NotificationType.Info));
                var result = await UserImageService.UpdateUserImage(SelectedImg);
                if (result.Flag)
                {
                    NotificationsService.PushNotification(new ApplicationLayer.Notification(result.Message, NotificationType.Success));
                    StateHasChanged();
                }
                else
                {
                    NotificationsService.PushNotification(new ApplicationLayer.Notification(result.Message, NotificationType.Error));
                }
            }
            catch
            {
                NotificationsService.PushNotification(new ApplicationLayer.Notification("An error occured please try again later.", NotificationType.Error));
            }
            finally
            {
                DisableImg = false;
            }
            
        }
    }
}
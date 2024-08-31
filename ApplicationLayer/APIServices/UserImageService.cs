using ApplicationLayer.Interfaces;
using Azure.Storage.Blobs;
using DomainLayer.Constants;
using DomainLayer.Entities;
using DomainLayer.Entities.APIClasses;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ApplicationLayer.APIServices;

public class UserImageService(IOptions<AzureSettings>azureSettings, UserManager<ApplicationUser>userManager) : IUserImageService
{
    public async Task<string> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest)
    {
        //SETUP AZURE CLIENT
        var blobServiceClient = new BlobServiceClient(azureSettings.Value.ConnectionString);
        var containerClient = updateUserImageRequest.TypeOfImage == Constants.ImageTypeOfUpdate.ProfileImage
            ? blobServiceClient.GetBlobContainerClient(azureSettings.Value.ProfileImgContainer)
            : blobServiceClient.GetBlobContainerClient(azureSettings.Value.BackgroundImgContainer);

        await containerClient.CreateIfNotExistsAsync();
        
        //CHECK IF USER ALREADY HAS AN IMAGE
        var user = await userManager.FindByNameAsync(updateUserImageRequest.Username);
        if (user != null)
        {
            var imageUrl = updateUserImageRequest.TypeOfImage == Constants.ImageTypeOfUpdate.ProfileImage
                ? user.ProfileImageUrl
                : user.BackgroundImageUrl;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var imageExt = Path.GetExtension(imageUrl);
                var existingBlobClient = containerClient.GetBlobClient($"{updateUserImageRequest.Username}{imageExt}");
                await existingBlobClient.DeleteIfExistsAsync();
            }
        }
        var fileExtension = Path.GetExtension(updateUserImageRequest.FileName)?.ToLower();
        //Upload To Azure

        var blobClient = containerClient.GetBlobClient($"{updateUserImageRequest.Username}{fileExtension}");

        await blobClient.UploadAsync(updateUserImageRequest.ImageStream, overwrite: true);

        //Update User in DB
        if (user != null)
        {
            if(updateUserImageRequest.TypeOfImage == Constants.ImageTypeOfUpdate.ProfileImage)
                user.ProfileImageUrl = blobClient.Uri.ToString();
            if(updateUserImageRequest.TypeOfImage == Constants.ImageTypeOfUpdate.BackgroundImage)
                user.BackgroundImageUrl = blobClient.Uri.ToString();
            await userManager.UpdateAsync(user);
        }
        return blobClient.Uri.ToString();
    }
}
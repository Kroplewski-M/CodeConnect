using ApplicationLayer.DTO_s;
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
    public async Task<ServiceResponse> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest)
    {
        var user = await userManager.FindByNameAsync(updateUserImageRequest.Username);
        if (user == null)
            return new ServiceResponse(false, "The user could not be found");
        
        //SETUP AZURE CLIENT
        var blobServiceClient = new BlobServiceClient(azureSettings.Value.ConnectionString);
        var containerClient = updateUserImageRequest.TypeOfImage == Consts.ImageType.ProfileImages
            ? blobServiceClient.GetBlobContainerClient(Consts.ImageType.ProfileImages.ToString().ToLower())
            : blobServiceClient.GetBlobContainerClient(Consts.ImageType.BackgroundImages.ToString().ToLower());
        
        await containerClient.CreateIfNotExistsAsync();
        
        //CHECK IF USER ALREADY HAS AN IMAGE
        await RemoveIfOldImageExists(user,updateUserImageRequest.TypeOfImage, containerClient);
        
        //Upload To Azure
        var fileExtension = Path.GetExtension(updateUserImageRequest.FileName)?.ToLower();
        var guid = Guid.NewGuid();
        var userImgName = $"{updateUserImageRequest.Username}-{guid}{fileExtension}";
        var blobClient = containerClient.GetBlobClient(userImgName);

        await blobClient.UploadAsync(updateUserImageRequest.ImageStream, overwrite: true);

        //Update User in DB
        await UpdateUserImage(user, updateUserImageRequest.TypeOfImage, userImgName);
        
        return new ServiceResponse(true, "Image updated successfully");
    }

    private async Task RemoveIfOldImageExists(ApplicationUser user, Consts.ImageType imageType, BlobContainerClient containerClient)
    {
            var imageUrl = imageType == Consts.ImageType.ProfileImages
                ? user.ProfileImageUrl
                : user.BackgroundImageUrl;
            var fileName = Path.GetFileName(imageUrl);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var existingBlobClient = containerClient.GetBlobClient(fileName);
                await existingBlobClient.DeleteIfExistsAsync();
            }
    }

    private async Task UpdateUserImage(ApplicationUser user, Consts.ImageType imageType, string url)
    {
        if(imageType == Consts.ImageType.ProfileImages)
            user.ProfileImageUrl = url;
        if(imageType == Consts.ImageType.BackgroundImages)
            user.BackgroundImageUrl = url;
        await userManager.UpdateAsync(user);
    }
}
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
        var containerClient = updateUserImageRequest.TypeOfImage == Consts.ImageTypeOfUpdate.ProfileImage
            ? blobServiceClient.GetBlobContainerClient(azureSettings.Value.ProfileImgContainer)
            : blobServiceClient.GetBlobContainerClient(azureSettings.Value.BackgroundImgContainer);
        
        await containerClient.CreateIfNotExistsAsync();
        
        //CHECK IF USER ALREADY HAS AN IMAGE
        await RemoveIfOldImageExists(user,updateUserImageRequest.TypeOfImage, containerClient);
        
        //Upload To Azure
        var fileExtension = Path.GetExtension(updateUserImageRequest.FileName)?.ToLower();
        var guid = Guid.NewGuid();
        var blobClient = containerClient.GetBlobClient($"{updateUserImageRequest.Username}-{guid}{fileExtension}");

        await blobClient.UploadAsync(updateUserImageRequest.ImageStream, overwrite: true);

        //Update User in DB
        await UpdateUserImage(user, updateUserImageRequest.TypeOfImage, blobClient.Uri.ToString());
        
        return new ServiceResponse(true, "Image updated successfully");
    }

    private async Task RemoveIfOldImageExists(ApplicationUser user, Consts.ImageTypeOfUpdate imageType, BlobContainerClient containerClient)
    {
            var imageUrl = imageType == Consts.ImageTypeOfUpdate.ProfileImage
                ? user.ProfileImageUrl
                : user.BackgroundImageUrl;
            var fileName = Path.GetFileName(imageUrl);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var existingBlobClient = containerClient.GetBlobClient(fileName);
                await existingBlobClient.DeleteIfExistsAsync();
            }
    }

    private async Task UpdateUserImage(ApplicationUser user, Consts.ImageTypeOfUpdate imageType, string url)
    {
        if(imageType == Consts.ImageTypeOfUpdate.ProfileImage)
            user.ProfileImageUrl = url;
        if(imageType == Consts.ImageTypeOfUpdate.BackgroundImage)
            user.BackgroundImageUrl = url;
        await userManager.UpdateAsync(user);
    }
}
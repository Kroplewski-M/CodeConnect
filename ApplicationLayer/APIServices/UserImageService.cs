using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DomainLayer.Constants;
using DomainLayer.Entities;
using DomainLayer.Entities.APIClasses;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ApplicationLayer.APIServices;

public class UserImageService(IOptions<AzureSettings>azureSettings, UserManager<ApplicationUser>userManager, AzureService azureService) : IUserImageService
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
        
        //await containerClient.CreateIfNotExistsAsync();
        
        //CHECK IF USER ALREADY HAS AN IMAGE
        await RemoveIfOldImageExists(user,updateUserImageRequest.TypeOfImage, containerClient);
        
        //Upload To Azure
        var fileExtension = Path.GetExtension(updateUserImageRequest.FileName)?.ToLower();
        if(string.IsNullOrEmpty(fileExtension))
            return new ServiceResponse(false, "issue occured getting file extension");
        var guid = Guid.NewGuid();
        var userImgName = $"{updateUserImageRequest.Username}-{guid}";
        
        var response = await azureService.UploadImage(updateUserImageRequest.TypeOfImage,updateUserImageRequest.ImgBase64,userImgName,fileExtension);
        
        if(!response.Flag)
            return response;
        //Update User in DB
        await UpdateUserImage(user, updateUserImageRequest.TypeOfImage, userImgName);
        return response;
    }

    private async Task RemoveIfOldImageExists(ApplicationUser user, Consts.ImageType imageType, BlobContainerClient containerClient)
    {
            var imageUrl = imageType == Consts.ImageType.ProfileImages
                ? user.ProfileImage
                : user.BackgroundImage;
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
            user.ProfileImage = url;
        if(imageType == Consts.ImageType.BackgroundImages)
            user.BackgroundImage = url;
        await userManager.UpdateAsync(user);
    }
}
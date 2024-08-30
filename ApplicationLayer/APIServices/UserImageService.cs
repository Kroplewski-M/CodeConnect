using ApplicationLayer.Interfaces;
using Azure.Storage.Blobs;
using DomainLayer.Constants;
using DomainLayer.Entities.APIClasses;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;

namespace ApplicationLayer.APIServices;

public class UserImageService(IOptions<AzureSettings>azureSettings) : IUserImageService
{
    public async Task<string> UpdateUserImage(Constants.ImageTypeOfUpdate imageType, IBrowserFile image)
    {
        using var memoryStream = new MemoryStream();
        await image.OpenReadStream().CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        var blobServiceClient = new BlobServiceClient(azureSettings.Value.ConnectionString);
        var containerClient = imageType == Constants.ImageTypeOfUpdate.ProfileImage ? 
            blobServiceClient.GetBlobContainerClient(azureSettings.Value.ProfileImgContainer)
            : blobServiceClient.GetBlobContainerClient(azureSettings.Value.BackgroundImgContainer);
        
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(image.Name);
        await blobClient.UploadAsync(memoryStream, overwrite: true);
        return blobClient.Uri.ToString();
    }
}
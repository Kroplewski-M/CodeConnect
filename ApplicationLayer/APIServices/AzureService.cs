using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DomainLayer.Constants;
using DomainLayer.Entities.APIClasses;
using Microsoft.Extensions.Options;

namespace ApplicationLayer.APIServices;

public class AzureService(BlobServiceClient blobServiceClient) : IAzureService
{

    public async Task<AzureImageDto> UploadImage(Consts.ImageType imageType,string base64Image, string imageName,string imageExt)
    {
        var blobContainerClient = GetBlobContainerClient(imageType);
        if(blobContainerClient == null)
            return new AzureImageDto(false, "","Upload Failed");
        
        await blobContainerClient.CreateIfNotExistsAsync();
        var fullImgName = $"{imageName}{imageExt}";
        var blobClient = blobContainerClient.GetBlobClient(fullImgName);
        
        var base64Data = base64Image.Contains(',') ? base64Image.Split(',')[1] : base64Image;

        // Convert Base64 string to byte array
        byte[] imageBytes = Convert.FromBase64String(base64Data);
        Response<BlobContentInfo>? response;
        using (MemoryStream stream = new MemoryStream(imageBytes))
        {
            // Upload the stream to Azure Blob Storage
            response = await blobClient.UploadAsync(stream, overwrite: true);
        }
        if(response == null)
            return new AzureImageDto(false, "","Upload Failed");
        return new AzureImageDto(true, fullImgName,"Image updated successfully");
    }

    public async Task<ServiceResponse> RemoveImage(string imageName, Consts.ImageType imageType)
    {
        var blobContainerClient = imageType == Consts.ImageType.ProfileImages
            ? blobServiceClient.GetBlobContainerClient(Consts.ImageType.ProfileImages.ToString().ToLower())
            : blobServiceClient.GetBlobContainerClient(Consts.ImageType.BackgroundImages.ToString().ToLower());
        var existingBlobClient = blobContainerClient.GetBlobClient(imageName);
        var result = await existingBlobClient.DeleteIfExistsAsync();
        return result == true ? new ServiceResponse(true, "Image deleted successfully") 
                                 : new ServiceResponse(false, "Image could not be deleted");
    }

    public BlobContainerClient? GetBlobContainerClient(Consts.ImageType imageType)
    {
        return imageType switch
        {
            Consts.ImageType.ProfileImages => blobServiceClient.GetBlobContainerClient(Consts.ImageType.ProfileImages
                .ToString()
                .ToLower()),
            Consts.ImageType.BackgroundImages => blobServiceClient.GetBlobContainerClient(Consts.ImageType
                .BackgroundImages.ToString()
                .ToLower()),
            Consts.ImageType.PostImages => blobServiceClient.GetBlobContainerClient(Consts.ImageType.PostImages
                .ToString()
                .ToLower()),
            _ => null
        };
    }
}
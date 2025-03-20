using ApplicationLayer.DTO_s;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DomainLayer.Constants;
using DomainLayer.Entities.APIClasses;
using Microsoft.Extensions.Options;

namespace ApplicationLayer.APIServices;

public class AzureService(IOptions<AzureSettings> azureSettings)
{
    private readonly BlobServiceClient _blobServiceClient = new(azureSettings.Value.ConnectionString);

    public async Task<ServiceResponse> UploadImage(Consts.ImageType imageType,string base64Image, string imageName,string imageExt)
    {
        var blobContainerClient = imageType == Consts.ImageType.ProfileImages
            ? _blobServiceClient.GetBlobContainerClient(Consts.ImageType.ProfileImages.ToString().ToLower())
            : _blobServiceClient.GetBlobContainerClient(Consts.ImageType.BackgroundImages.ToString().ToLower());
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
            return new ServiceResponse(false, "Upload Failed");
        return new ServiceResponse(true, "Image updated successfully");
    }
}
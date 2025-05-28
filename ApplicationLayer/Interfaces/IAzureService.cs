using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Images;
using Azure.Storage.Blobs;
using DomainLayer.Constants;

namespace ApplicationLayer.Interfaces;

public interface IAzureService
{
    public Task<AzureImageDto> UploadImage(Consts.ImageType imageType, string base64Image, string imageName,
        string imageExt);
    public Task<ServiceResponse> RemoveImage(string imageName, Consts.ImageType imageType);
    public BlobContainerClient? GetBlobContainerClient(Consts.ImageType imageType);
}
using System.Security.Authentication.ExtendedProtection;
using ApplicationLayer.APIServices;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.APIClasses;
using Microsoft.Extensions.Options;
using Moq;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DomainLayer.Constants;

namespace ApiServiceTests;

public class AzureServiceTests
{
    private readonly Mock<IOptions<AzureSettings>> _azureSettingsMock;
    private readonly Mock<BlobServiceClient> _blobServiceClient;
    private readonly Mock<BlobContainerClient> _blobContainerClient;
    private readonly Mock<BlobClient> _blobClient;
    private AzureService _azureService;

    public AzureServiceTests()
    {
        _azureSettingsMock = new Mock<IOptions<AzureSettings>>();
        _blobServiceClient = new Mock<BlobServiceClient>();
        _blobContainerClient = new Mock<BlobContainerClient>();
        _blobClient = new Mock<BlobClient>();
        _azureService = new AzureService(_blobServiceClient.Object);
    }

    // [Fact]
    // public async Task UploadImage_ShouldReturnSuccess_WhenImageUploaded()
    // {
    //     //Arrange
    //     var imageType = Consts.ImageType.ProfileImages;
    //     var base64Image = "base64Image";
    //     var imageName = "imageName";
    //     var imageExt = ".png";
    //     // Act
    //     
    //     //var result = await _azureService.UploadImage(imageType, base64Image, imageName, imageExt);
    //     
    //     // Assert
    //     // Assert.True(result.Flag);
    //     // Assert.Equal("testimage.png", result.ImageName);
    //     // Assert.Equal("Image updated successfully", result.Message);
    // }
}
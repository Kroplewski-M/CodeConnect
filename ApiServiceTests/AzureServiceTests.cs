using System.Security.Authentication.ExtendedProtection;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.APIClasses;
using Microsoft.Extensions.Options;
using Moq;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DomainLayer.Constants;
using WebApiApplicationLayer;
using WebApiApplicationLayer.Services;

namespace ApiServiceTests;

public class AzureServiceTests
{
    private readonly Mock<BlobServiceClient> _blobServiceClient;
    private readonly Mock<BlobContainerClient> _blobContainerClient;
    private readonly Mock<BlobClient> _blobClient;
    private AzureService _azureService;

    public AzureServiceTests()
    {
        _blobServiceClient = new Mock<BlobServiceClient>();
        _blobContainerClient = new Mock<BlobContainerClient>();
        _blobClient = new Mock<BlobClient>();
        
        _blobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>())).Returns(_blobContainerClient.Object);
        _blobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>())).Returns(_blobClient.Object); 
        _blobContainerClient.Setup(x => x.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(),null,null,CancellationToken.None))
            .ReturnsAsync(Mock.Of<Response<BlobContainerInfo>>());
        _blobClient.Setup(x => x.UploadAsync(It.IsAny<Stream>(), true, CancellationToken.None))
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());
        _azureService = new AzureService(_blobServiceClient.Object);
    }

    [Fact]
    public async Task UploadImage_ShouldReturnSuccess_WhenImageUploaded()
    {
        // Arrange
        var imageType = Consts.ImageType.ProfileImages;
        var base64Image = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }); // valid base64
        var imageName = "testimage";
        var imageExt = ".png";
        var expectedFullName = "testimage.png";

        // Act
        var result = await _azureService.UploadImage(imageType, base64Image, imageName, imageExt);

        // Assert
        Assert.True(result.Flag);
        Assert.Equal(expectedFullName, result.ImageName);
        Assert.Equal("Image updated successfully", result.Message);
    }
    [Fact]
    public async Task UploadImage_EmptyImage_ShouldReturnFailure_WhenImageUploaded()
    {
        // Arrange
        var imageType = Consts.ImageType.ProfileImages;
        var base64Image = "";
        var imageName = "testimage";
        var imageExt = ".png";
        // Act
        var result = await _azureService.UploadImage(imageType, base64Image, imageName, imageExt);
        // Assert
        Assert.False(result.Flag);
        Assert.Equal("Upload Failed", result.Message);
    }
    [Fact]
    public async Task UploadImage_NoImageName_ShouldReturnFailure_WhenImageUploaded()
    {
        // Arrange
        var imageType = Consts.ImageType.ProfileImages;
        var base64Image = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }); // valid base64
        var imageName = "";
        var imageExt = ".png";
        // Act
        var result = await _azureService.UploadImage(imageType, base64Image, imageName, imageExt);
        // Assert
        Assert.False(result.Flag);
        Assert.Equal("Upload Failed", result.Message);
    }
    [Fact]
    public async Task UploadImage_NoImageExt_ShouldReturnFailure_WhenImageUploaded()
    {
        // Arrange
        var imageType = Consts.ImageType.ProfileImages;
        var base64Image = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }); // valid base64
        var imageName = "TestName";
        var imageExt = "";
        // Act
        var result = await _azureService.UploadImage(imageType, base64Image, imageName, imageExt);
        // Assert
        Assert.False(result.Flag);
        Assert.Equal("Upload Failed", result.Message);
    }

    [Fact]
    public async Task DeleteImage_ShouldReturnSuccess_WhenImageDeleted()
    {
        //Arrange
        var imageName = "TestName";
        _blobContainerClient.Setup(x => x.GetBlobClient(imageName)).Returns(_blobClient.Object);
        _blobClient
            .Setup(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));
        
        //Act
        var result = await _azureService.RemoveImage(imageName, Consts.ImageType.ProfileImages);
        //Assert
        Assert.True(result.Flag);
        Assert.Equal("Image deleted successfully", result.Message);
    }
    [Fact]
    public async Task RemoveImage_ShouldReturnFailure_WhenImageDoesNotExist()
    {
        //Arrange
        _blobClient
            .Setup(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));
        //Act
        var result = await _azureService.RemoveImage("nonexistent.png", Consts.ImageType.ProfileImages);
        //Assert
        Assert.False(result.Flag);
        Assert.Equal("Image could not be deleted", result.Message);
    }
}
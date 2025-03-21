using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.APIServices;

public class UserImageService(UserManager<ApplicationUser>userManager, AzureService azureService) : IUserImageService
{
    public async Task<ServiceResponse> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest)
    {
        var user = await userManager.FindByNameAsync(updateUserImageRequest.Username);
        if (user == null)
            return new ServiceResponse(false, "The user could not be found");
        
        //CHECK IF USER ALREADY HAS AN IMAGE
        await RemoveIfOldImageExists(user,updateUserImageRequest.TypeOfImage);
        
        //Upload To Azure
        var fileExtension = Path.GetExtension(updateUserImageRequest.FileName).ToLower();
        if(string.IsNullOrEmpty(fileExtension))
            return new ServiceResponse(false, "issue occured getting file extension");
        var guid = Guid.NewGuid();
        var userImgName = $"{updateUserImageRequest.Username}-{guid}";
        
        var response = await azureService.UploadImage(updateUserImageRequest.TypeOfImage,updateUserImageRequest.ImgBase64,userImgName,fileExtension);

        if (response.Flag)
        {
            //Update User in DB
            await UpdateUserImage(user, updateUserImageRequest.TypeOfImage, response.ImageName);   
        }
        return new ServiceResponse(response.Flag,response.Message);
    }

    private async Task RemoveIfOldImageExists(ApplicationUser user, Consts.ImageType imageType)
    {
            var imageUrl = imageType == Consts.ImageType.ProfileImages
                ? user.ProfileImage
                : user.BackgroundImage;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await azureService.RemoveImage(imageUrl, imageType);
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
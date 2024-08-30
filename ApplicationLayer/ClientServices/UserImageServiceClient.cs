using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Components.Forms;

namespace ApplicationLayer.ClientServices;

public class UserImageServiceClient: IUserImageService
{
    public Task<string> UpdateUserImage(Constants.ImageTypeOfUpdate imageType, IBrowserFile image)
    {
        throw new NotImplementedException();
    }
}
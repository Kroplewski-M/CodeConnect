using DomainLayer.Constants;
using Microsoft.AspNetCore.Components.Forms;

namespace ApplicationLayer.Interfaces;

public interface IUserImageService
{
    public Task<string> UpdateUserImage(Constants.ImageTypeOfUpdate imageType, IBrowserFile image);
}
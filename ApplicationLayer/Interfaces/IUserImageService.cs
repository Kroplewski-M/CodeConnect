using DomainLayer.Constants;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components.Forms;

namespace ApplicationLayer.Interfaces;

public interface IUserImageService
{
    public Task<string> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest);
}
using ApplicationLayer.DTO_s;
using DomainLayer.Constants;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components.Forms;

namespace ApplicationLayer.Interfaces;

public interface IUserImageService
{
    public Task<ServiceResponse> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest, string? userId = null);
}
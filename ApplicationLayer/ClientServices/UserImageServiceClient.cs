using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Components.Forms;

namespace ApplicationLayer.ClientServices;

public class UserImageServiceClient(HttpClient httpClient): IUserImageService
{
    public async Task<string> UpdateUserImage(UpdateUserImageRequest updateUserImageRequest)
    {
        using var content = new MultipartFormDataContent();
        
        var fileContent = new StreamContent(updateUserImageRequest.ImageStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(updateUserImageRequest.ContentType);
        content.Add(fileContent, "image", updateUserImageRequest.FileName);
        content.Add(new StringContent(((int)updateUserImageRequest.TypeOfImage).ToString()), "typeOfImage");
        var response = await httpClient.PostAsync("/api/User/UpdateUserImage", content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
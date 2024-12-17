

namespace DomainLayer.Entities;

public class UpdateUserImageRequest
{
    public Constants.Consts.ImageTypeOfUpdate TypeOfImage { get; set; }
    public Stream? ImageStream { get; set; }  // This will be filled in by either IBrowserFile or IFormFile
    public string ContentType { get; set; } = "";  // Store content type as a string
    public string Username { get; set; } = "";
    public string FileName { get; set; } = ""; 
}


namespace DomainLayer.Entities;

public class UpdateUserImageRequest
{
    public Constants.Constants.ImageTypeOfUpdate TypeOfImage { get; init; }
    public Stream? ImageStream { get; init; }  // This will be filled in by either IBrowserFile or IFormFile
    public string ContentType { get; init; } = "";  // Store content type as a string
    public string Username { get; init; } = "";
    public string FileName { get; init; } = ""; // Store the file name
}


namespace DomainLayer.Entities;

public class UpdateUserImageRequest
{
    public Constants.Consts.ImageType TypeOfImage { get; set; }
    public string ImgBase64 { get; set; } = "";
    public string Username { get; set; } = "";
    public string FileName { get; set; } = ""; 
}
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components;

public enum TypeOfUpdate
{
    ProfileImage,
    BackgroundImage
}

public class UpdateImageBase : ComponentBase
{
    [Parameter]
    public TypeOfUpdate UpdateOfType { get; set; }
    [Parameter]
    public EventCallback Cancel { get; set; }
    
    protected string typeOfUploadedImg { get; set; }
    protected string UploadedImgBase64 { get; set; }
}
using DomainLayer.Constants;

namespace DomainLayer.Helpers;

public static class Helpers
{
    public static string GetAzureImgUrl(Consts.ImageType imgType ,string imgName)
    {
        return $"{Consts.AzureBlobEndpoint}/{imgType.ToString().ToLower()}/{imgName}";
    }
}
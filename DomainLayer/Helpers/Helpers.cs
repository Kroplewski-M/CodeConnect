using DomainLayer.Constants;

namespace DomainLayer.Helpers;

public static class Helpers
{
    public static string GetAzureImgUrl(Consts.ImageType imgType ,string imgName)
    {
        return $"{Consts.AzureBlobEndpoint}/{imgType.ToString().ToLower()}/{imgName}";
    }
    public static bool IsBase64(string base64)
    {   
        if (string.IsNullOrWhiteSpace(base64))
            return false;
        Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer , out int bytesParsed);
    }
}
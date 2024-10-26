using DomainLayer.Constants;
using Microsoft.AspNetCore.Components.Forms;

namespace ApplicationLayer.ClientServices;

public class ImageConvertorServiceClient
{
    public async Task<string> ImageToBase64(IBrowserFile browserFile)
    {
        using (var stream = ImageToStream(browserFile))
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                string base64 = Convert.ToBase64String(imageBytes);
                return Base64ToImageData(base64);
            }
        }
    }

    public async Task<string> StreamToBase64(Stream streamData)
    {
        await using (var stream = streamData)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                string base64 = Convert.ToBase64String(imageBytes);
                Console.WriteLine(base64);
                return Base64ToImageData(base64);
            }
        }
    }
    public string Base64ToImageData(string base64)
    {
        var imageType = base64.Split('/')[1];
        return $"data:image/{imageType};base64,{base64}";
    }
    public Stream ImageToStream(IBrowserFile browserFile)
    {
        var maxAllowedSize = Constants.Base.MaxFileSize;
         return browserFile.OpenReadStream(maxAllowedSize);
    }
}
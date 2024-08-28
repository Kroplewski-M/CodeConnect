using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

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

    protected string TempFilePath { get; set; } = ""; 
    protected async Task HandleFileSelection(InputFileChangeEventArgs e)
    {
        string result = Path.GetTempPath();
        Console.WriteLine(result);
        var file = e.GetMultipleFiles().FirstOrDefault();
        if (file != null)
        {
            string tempFileName = "temp" + Path.GetExtension(file.Name);
            TempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);
            await using (var stream = file.OpenReadStream())
            {
                await using (var fileStream = new FileStream(TempFilePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
            Console.WriteLine($"File saved to: {TempFilePath}");
        }
        StateHasChanged();
    }
}
using System.Reflection.Metadata;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CodeConnect.WebAssembly.Components;

public class UpdateImageBase : ComponentBase
{
    [Inject]
    public IJSRuntime Js { get; set; }
    [Parameter]
    public Constants.ImageTypeOfUpdate UpdateOfImageType { get; set; }
    [Parameter]
    public EventCallback Cancel { get; set; }

    protected bool LoadedImg { get; set; } = false;
    protected IBrowserFile? SelectedImg { get; set; } = null;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
            await Js.InvokeVoidAsync("PreviewImg",SelectedImg);
    }
    protected void HandleFileSelection(InputFileChangeEventArgs e)
    {
        SelectedImg = e.GetMultipleFiles().FirstOrDefault();
        LoadedImg = true;
    }

    protected void SaveImg()
    {
        Console.WriteLine(SelectedImg?.Name);
    }
}
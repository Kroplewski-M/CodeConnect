using System.Net.Http;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages;

public class HomeBase : ComponentBase
{
    public string hello = string.Empty;
    protected override async Task OnInitializedAsync()
    {
        try
        {
            HttpClient client = new HttpClient();
            hello = await client.GetStringAsync("https://localhost:7124/api/HelloWorld");
        }
        catch (Exception err)
        {
            Console.Write(err);
        }
    }
}
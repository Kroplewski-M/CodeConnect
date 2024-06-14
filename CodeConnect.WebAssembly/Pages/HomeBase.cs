using System.Net.Http;
using System.Net.Http.Json;
using DomainLayer.Entities.Auth;
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
            var user = new RegisterFormViewModel
            {
                FirstName = "Mateusz",
                LastName = "Kroplewski",
                Email = "kroplewski@gmail.com",
                DOB = new DateTime(1990, 1, 1),
                Password = "password123"
            };
            var result = await client.PostAsJsonAsync("https://localhost:7124/api/Authentication/CreateUser",user);
        }
        catch (Exception err)
        {
            Console.Write(err);
        }
    }
}
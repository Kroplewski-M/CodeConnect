using System.Net.Http;
using System.Net.Http.Json;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages;

public class HomeBase : ComponentBase
{
    [Inject]
    protected IAuthenticateService authenticateService { get; set; }
    protected override async Task OnInitializedAsync()
    {
        var user = new RegisterFormViewModel
        {
            FirstName = "Mateusz",
            LastName = "Kroplewski",
            Email = "Maty@gmail.com",
            DOB = new DateTime(1990, 1, 1),
            Password = "Password123!"
        };
        try
        {
            
            var result = await authenticateService.CreateUser(user);
        }
        catch (Exception err)
        {
            Console.Write(err);
        }
    }
}
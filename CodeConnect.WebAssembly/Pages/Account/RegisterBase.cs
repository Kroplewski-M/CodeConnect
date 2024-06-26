using System.Net.Http;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.Validators;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages.Account;

public class RegisterBase : ComponentBase
{
    public RegisterForm RegisterForm = new RegisterForm();

    public async Task SubmitRegister()
    {
        RegisterFormValidator registerFormValidator = new RegisterFormValidator();
        var validate = registerFormValidator.Validate(RegisterForm);
    }
    
}
using System.Net.Http;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.Validators;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages.Account;

public class RegisterBase : ComponentBase
{
    public RegisterForm RegisterForm = new RegisterForm();
    public List<ValidationFailure> RegisterErrors = new List<ValidationFailure>();
    public async Task SubmitRegister()
    {
        RegisterFormValidator RegisterFormValidator = new RegisterFormValidator();
        var validate = await RegisterFormValidator.ValidateAsync(RegisterForm);
        if (!validate.IsValid)
        {
            RegisterErrors = validate.Errors;
            StateHasChanged();
        }
        else
        {
            RegisterErrors = [];
        }
    }
    
}
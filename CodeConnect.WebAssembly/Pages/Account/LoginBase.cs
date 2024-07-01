using System.Net.Http;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.Validators;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages.Account;

public class LoginBase : ComponentBase
{
    public LoginForm LoginForm = new LoginForm();
    public List<ValidationFailure> LoginErrors = new List<ValidationFailure>();

    public async Task SubmitLogin()
    {
        LoginFormValidator loginFormValidator = new LoginFormValidator();
        var validate = await loginFormValidator.ValidateAsync(LoginForm);
        
        if (!validate.IsValid)
        {
            LoginErrors = validate.Errors;
            StateHasChanged();
        }
        else
        {
            LoginErrors = [];
        }
    }
    
}
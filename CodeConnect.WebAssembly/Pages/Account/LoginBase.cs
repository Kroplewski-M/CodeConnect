using System.Net.Http;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.Validators;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages.Account;

public class LoginBase : ComponentBase
{
    public LoginForm LoginForm = new LoginForm();

    public async Task SubmitLogin()
    {
        LoginFormValidator loginFormValidator = new LoginFormValidator();
        var validate = loginFormValidator.Validate(LoginForm);
    }
    
}
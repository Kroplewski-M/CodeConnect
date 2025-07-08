using System.Net.Http;
using ApplicationLayer;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages.Account;

public class LoginBase : ComponentBase
{
    [Inject]
    public required ToastService ToastService { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required IAuthenticateServiceClient AuthenticateServiceClient { get; set; }

    public LoginForm LoginForm = new LoginForm();
    public List<ValidationFailure> LoginErrors = new List<ValidationFailure>();
    public bool DisableLogin { get; set; } = false;
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
            try
            {
                DisableLogin = true;
                ToastService.PushToast(new Toast("Logging in",
                    ToastType.Info));
                var result = await AuthenticateServiceClient.LoginUser(LoginForm);
                if (!result.Flag)
                {
                    LoginErrors.Add(new ValidationFailure
                    { PropertyName = "ResponseError", ErrorMessage = result.Message });
                    LoginForm.Password = "";
                    StateHasChanged();
                }
                else
                {
                    ToastService.PushToast(new Toast("Account found!",
                        ToastType.Success));
                    NavigationManager.NavigateTo("/MyFeed");
                }
            }
            catch
            {
                ToastService.PushToast(new Toast("Error Occured During Log In Please Try Again Later.",
                    ToastType.Error));
            }
            finally
            {
                DisableLogin = false;
            }
        }
    }

}

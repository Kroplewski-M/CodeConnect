using System.Net.Http;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.Validators;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages.Account;

public class LoginBase : ComponentBase
{
    [Inject]
    public required NotificationsService NotificationsService { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required IAuthenticateService AuthenticateService { get; set; }

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
                NotificationsService.PushNotification(new Notification("Logging in",
                    NotificationType.Info));
                var result = await AuthenticateService.LoginUser(LoginForm);
                if (!result.Flag)
                {
                    LoginErrors.Add(new ValidationFailure
                    { PropertyName = "ResponseError", ErrorMessage = result.Message });
                    StateHasChanged();
                }
                else
                {
                    NotificationsService.PushNotification(new Notification("Account found!",
                        NotificationType.Success));
                    NavigationManager.NavigateTo("/MyFeed");
                }
            }
            catch
            {
                NotificationsService.PushNotification(new Notification("Error Occured During Registering Please Try Again Later.",
                    NotificationType.Error));
            }
            finally
            {
                DisableLogin = false;
            }
        }
    }

}

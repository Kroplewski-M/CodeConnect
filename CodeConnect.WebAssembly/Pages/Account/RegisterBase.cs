using System.Net.Http;
using ApplicationLayer.ClientServices;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.Validators;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CodeConnect.WebAssembly.Pages.Account;

public class RegisterBase
    : ComponentBase
{
    [Inject]
    public required NotificationsService NotificationsService { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required IAuthenticateService AuthenticateService { get; set; }

    public RegisterForm RegisterForm = new RegisterForm();
    protected List<ValidationFailure> RegisterErrors = [];
    public bool DisableRegister { get; set; } = false;
    public async Task SubmitRegister()
    {
        RegisterFormValidator registerFormValidator = new RegisterFormValidator();
        var validate = await registerFormValidator.ValidateAsync(RegisterForm);
        if (!validate.IsValid)
        {
            RegisterErrors = validate.Errors;
            StateHasChanged();
        }
        else
        {
            RegisterErrors = [];
            NotificationsService.PushNotification(new Notification("Creating Account", NotificationType.Info));
            try
            {
                DisableRegister = true;
                var result = await AuthenticateService.CreateUser(RegisterForm);
                if (!result.Flag)
                {
                    RegisterErrors.Add(new ValidationFailure
                    { PropertyName = "ResponseError", ErrorMessage = result.Message });
                    StateHasChanged();
                }
                else
                {
                    NotificationsService.PushNotification(new Notification("Account Created, Welcome to Code Connect!",
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
                DisableRegister = false;
            }

        }
    }

}

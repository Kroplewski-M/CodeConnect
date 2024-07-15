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
    public NotificationsService NotificationsService { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Inject]
    public IAuthenticateService AuthenticateService { get; set; } 

    public RegisterForm RegisterForm = new RegisterForm();
    protected List<ValidationFailure> RegisterErrors = [];
    public bool DisalbleRegister { get; set; } = false;
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
            NotificationsService.PushNotification(new Notification("Creating Account",NotificationType.Info));
            try
            {
                DisalbleRegister = true;
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
                    NavigationManager.NavigateTo("/");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
            }
            finally
            {
                DisalbleRegister = false;
            }
            
        }
    }
    
}
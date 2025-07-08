using System.Net.Http;
using ApplicationLayer;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CodeConnect.WebAssembly.Pages.Account;

public class RegisterBase
    : ComponentBase
{
    [Inject]
    public required ToastService ToastService { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required IAuthenticateServiceClient AuthenticateServiceClient { get; set; }

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
            ToastService.PushToast(new Toast("Creating Account", ToastType.Info));
            try
            {
                DisableRegister = true;
                var result = await AuthenticateServiceClient.CreateUser(RegisterForm);
                if (!result.Flag)
                {
                    RegisterErrors.Add(new ValidationFailure
                    { PropertyName = "ResponseError", ErrorMessage = result.Message });
                }
                else
                {
                    ToastService.PushToast(new Toast("Account Created, Welcome to Code Connect!",
                        ToastType.Success));
                    NavigationManager.NavigateTo("/MyFeed");
                }
            }
            catch
            {
                ToastService.PushToast(new Toast("Error Occured During Registering Please Try Again Later.",
                    ToastType.Error));
            }
            finally
            {
                DisableRegister = false;
                StateHasChanged();
            }

        }
    }

}

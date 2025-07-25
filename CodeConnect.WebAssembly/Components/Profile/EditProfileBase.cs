using ApplicationLayer;
using ApplicationLayer.Classes;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CodeConnect.WebAssembly.Components.Profile;

public class EditProfileBase : ComponentBase
{
    [Inject] public required  IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    [Inject] public required ToastService ToastService { get; set; }
    [Inject] public required IUserService UserService { get; set; }
    
    [Parameter] public EventCallback Cancel { get; set; }

    protected List<ValidationFailure> EditProfileErrors = [];

    
    protected EditProfileForm EditProfileForm = new EditProfileForm();
    [CascadingParameter] public required UserState UserState { get; set; }
    
    protected override void OnInitialized()
    {
        EditProfileForm = new EditProfileForm
        {
            FirstName = UserState?.Current?.FirstName ?? "",
            LastName = UserState?.Current?.LastName ?? "",
            GithubLink = UserState?.Current?.GithubLink ?? "",
            WebsiteLink = UserState?.Current?.WebsiteLink ?? "",
            Dob = UserState?.Current?.Dob ?? null,
            Bio = UserState?.Current?.Bio ?? "",
        };
        StateHasChanged();
    }

    public bool DisableEdit = false;
    protected async Task ConfirmEditProfile()
    {
        EditProfileErrors = [];
        EditProfileValidator editProfileValidator = new EditProfileValidator();
        var validate = await editProfileValidator.ValidateAsync(EditProfileForm);
        if (!validate.IsValid)
        {
            EditProfileErrors = validate.Errors;
            return;
        }
        try
        {
            DisableEdit = true;
            ToastService.PushToast(new ApplicationLayer.Toast("Updating Profile...",
                ToastType.Info));
            await UserService.UpdateUserDetails(EditProfileForm);
            ToastService.PushToast(new ApplicationLayer.Toast("Updated Profile Successfully!",
                ToastType.Success));
        }
        catch
        {
            ToastService.PushToast(new ApplicationLayer.Toast("Error while updating profile details, please try again later.",
                ToastType.Error));
        }
        finally
        {
            DisableEdit = false;
        }
    }
    
}
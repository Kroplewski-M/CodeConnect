using ApplicationLayer;
using ApplicationLayer.Classes;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class VerifyDobBase : ComponentBase
{
    [CascadingParameter] public required UserState UserState { get; set; }
    [Inject] public required IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    [Inject] public required IUserService UserService { get; set; }
    [Inject] public required ToastService ToastService { get; set; }
    protected bool Loading { get; set; } = false;
    protected DateOnly? Dob { get; set; }
    protected List<ValidationFailure> EditProfileErrors = [];
    protected async Task SaveDob()
    {
        if(UserState.Current == null) return;
        try
        {
            Loading = true;
            var editProfileForm = new EditProfileForm
            {
                FirstName = UserState.Current.FirstName,
                LastName = UserState.Current.LastName,
                GithubLink = UserState.Current.GithubLink,
                WebsiteLink = UserState.Current.WebsiteLink,
                Dob = Dob,
                Bio = UserState.Current.Bio,
            };
            EditProfileErrors = [];
            EditProfileValidator editProfileValidator = new EditProfileValidator();
            var validate = await editProfileValidator.ValidateAsync(editProfileForm);
            if (!validate.IsValid)
            {
                EditProfileErrors = validate.Errors.Where(x => x.PropertyName == Consts.ClaimTypes.Dob).ToList();
            }
            else
            {
                await UserService.UpdateUserDetails(editProfileForm);
                ToastService.PushToast(new ApplicationLayer.Toast("Dob verified", ToastType.Success));
            }
        }
        catch
        {
            ToastService.PushToast(new ApplicationLayer.Toast("An error occured please try again later.", ToastType.Error));
        }
        finally
        {
            Loading = false;
            StateHasChanged();
        }

    }

    protected async Task Logout()
    {
        await AuthenticateServiceClient.LogoutUser();
    }
}
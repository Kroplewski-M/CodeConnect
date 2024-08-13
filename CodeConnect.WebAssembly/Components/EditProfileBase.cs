using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities;
using DomainLayer.Entities.Validators;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CodeConnect.WebAssembly.Components;

public class EditProfileBase : ComponentBase
{
    [Inject]
    public required  IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    [Inject]
    public required NotificationsService NotificationsService { get; set; }
    [Inject]
    public required IUserService UserService { get; set; }
    
    [Parameter]
    public EventCallback Cancel { get; set; }

    protected List<ValidationFailure> EditProfileErrors = [];

    
    protected EditProfileForm EditProfileForm = new EditProfileForm();
    private UserDetails? _userDetails;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            var user = authState?.User;

            if (user?.Identity is not null && user.Identity.IsAuthenticated)
            {
                _userDetails = AuthenticateServiceClient.GetUserFromFromAuthState(authState);
                EditProfileForm = new EditProfileForm
                {
                    FirstName = _userDetails.FirstName,
                    LastName = _userDetails.LastName,
                    DOB = _userDetails.Dob,
                    Bio = _userDetails.Bio,
                };
                StateHasChanged();
            }
        }
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
            NotificationsService.PushNotification(new DomainLayer.Entities.Notification("Updating Profile...",
                NotificationType.Info));
            //Post edit profile
            await UserService.UpdateUserDetails(EditProfileForm);
        }
        catch
        {
            NotificationsService.PushNotification(new DomainLayer.Entities.Notification("Error while updating profile details, please try again later.",
                NotificationType.Error));
        }
        finally
        {
            DisableEdit = false;
        }
    }
    
}
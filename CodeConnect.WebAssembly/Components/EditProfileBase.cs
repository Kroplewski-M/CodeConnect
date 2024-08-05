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
    
    [Parameter]
    public EventCallback Cancel { get; set; }

    protected List<ValidationFailure> EditProfileErrors = [];

    
    protected EditProfileForm EditProfileForm = new EditProfileForm();
    private UserDetails _userDetails;

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
                    FirstName = _userDetails.firstName,
                    LastName = _userDetails.lastName,
                    DOB = _userDetails.DOB,
                    Bio = _userDetails.bio,
                };
                StateHasChanged();
            }
        }
    }

    public bool disableEdit = false;
    protected async Task ConfirmEditProfile()
    {
        Console.WriteLine("FirstName: " + EditProfileForm.FirstName);
        Console.WriteLine("LastName: " + EditProfileForm.LastName);
        Console.WriteLine("DOB: " + EditProfileForm.DOB);
        Console.WriteLine("Bio: " + EditProfileForm.Bio);
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
            disableEdit = true;
            NotificationsService.PushNotification(new DomainLayer.Entities.Notification("Updating Profile...",
                NotificationType.Info));
        }
        catch
        {
            NotificationsService.PushNotification(new DomainLayer.Entities.Notification("Error while updating profile details, please try again later.",
                NotificationType.Error));
        }
        finally
        {
            disableEdit = false;
        }
    }
    
}
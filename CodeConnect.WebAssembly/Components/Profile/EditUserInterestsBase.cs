using ApplicationLayer;
using ApplicationLayer.APIServices;
using ApplicationLayer.ClientServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Azure;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class EditUserInterestsBase : ComponentBase
{
    [Inject]
    public required IUserService UserService { get; set; }
    [Parameter]
    public EventCallback Cancel { get; set; }

    [Parameter] public List<TechInterestsDto>? CurrentUserInterests { get; set; }
    
    [Inject]
    public required NotificationsService NotificationsService { get; set; }

    protected List<TechInterestsDto> AllTechInterests { get; set; } = new List<TechInterestsDto>();
    protected List<string> TechTypes { get; set; } = new();
    
    protected bool FetchingInterests = true;
    protected string? SelectedTechType { get; private set; } = string.Empty;
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (CurrentUserInterests == null)
            CurrentUserInterests = new List<TechInterestsDto>();
        try
        {
            AllTechInterests = await UserService.GetAllInterests();
            if (!AllTechInterests.Any())
            {
                throw new RequestFailedException("Failed to fetch tech interests");
            }
        }
        catch
        {
            NotificationsService.PushNotification(
                new ApplicationLayer.Notification("Failed to fetch interests", NotificationType.Error));
        }
        finally
        {
            FetchingInterests = false;
        }
        TechTypes = AllTechInterests.Select(x=>x.InterestType).Distinct().ToList(); 
        SelectedTechType = TechTypes.FirstOrDefault();
    }
    protected string InterestsErrorMessage { get; set; } = string.Empty;
    protected void AddInterest(TechInterestsDto interest)
    {
        InterestsErrorMessage = string.Empty;
        if (CurrentUserInterests != null && CurrentUserInterests.Count < 10)
        {
            CurrentUserInterests.Add(interest);
        }
        else
        {
            InterestsErrorMessage = "Max amount of interests is reached!";
        }
        StateHasChanged();
    }
    protected void ChangeTechType(string? selectedTechType)
    {
        SelectedTechType = selectedTechType;
        StateHasChanged();
    }

    protected void RemoveInterest(TechInterestsDto interest)
    {
        InterestsErrorMessage = string.Empty;
        CurrentUserInterests?.Remove(interest);
        StateHasChanged();
    }
    protected bool Saving = false;
    protected async Task UpdateInterests()
    {
        try
        {
            Saving = true;
            NotificationsService.PushNotification(
                new ApplicationLayer.Notification("Updating interests", NotificationType.Info));
            if (CurrentUserInterests != null)
            {
                var result = await UserService.UpdateUserInterests("", CurrentUserInterests);
                if (result.Flag)
                {
                    NotificationsService.PushNotification(
                        new ApplicationLayer.Notification(result.Message, NotificationType.Success));
                    StateHasChanged();
                    return;
                }

                NotificationsService.PushNotification(
                    new ApplicationLayer.Notification(result.Message, NotificationType.Error));
            }
        }
        catch
        {
            NotificationsService.PushNotification(
                new ApplicationLayer.Notification("An error occured please try again later.", NotificationType.Error));
        }
        finally
        {
            Saving = false;
        }
    }
}
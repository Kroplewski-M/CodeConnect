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
    public IUserService UserService { get; set; }
    [Parameter]
    public EventCallback Cancel { get; set; } 
    [Parameter]
    public UserInterestsDto? CurrentUserInterests { get; set; }
    
    [Inject]
    public NotificationsService NotificationsService { get; set; }
    protected List<TechInterestsDto> AllTechInterests { get; set; }
    protected List<string>TechTypes { get; set; } = new List<string>();
    
    protected bool fetchingInterests = true;
    protected string SelectedTechType { get; set; } = string.Empty;
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
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
            fetchingInterests = false;
        }
        TechTypes = AllTechInterests.Select(x=>x.InterestType).Distinct().ToList();
        SelectedTechType = TechTypes.FirstOrDefault();
    }

    protected void ChangeTechType(string selectedTechType)
    {
        SelectedTechType = selectedTechType;
        StateHasChanged();
    }
}
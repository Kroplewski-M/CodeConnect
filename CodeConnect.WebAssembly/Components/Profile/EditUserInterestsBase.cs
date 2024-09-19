using ApplicationLayer;
using ApplicationLayer.APIServices;
using ApplicationLayer.ClientServices;
using ApplicationLayer.DTO_s;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class EditUserInterestsBase : ComponentBase
{
    [Parameter]
    public EventCallback Cancel { get; set; } 
    [Parameter]
    public UserInterestsDto? CurrentUserInterests { get; set; }
    
    [Inject]
    public UserServiceClient UserService { get; set; }
    [Inject]
    public NotificationsService NotificationsService { get; set; }
    public List<TechInterestsDto> AllTechInterests { get; set; }
    public List<string>TechTypes { get; set; } = new List<string>();
    
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        try
        {
            AllTechInterests = await UserService.GetAllInterests();
        }
        catch
        {
            NotificationsService.PushNotification(new ApplicationLayer.Notification("Failed to fetch interests", NotificationType.Error));
        }
        TechTypes = AllTechInterests.Select(x=>x.InterestType).Distinct().ToList();
    }
    
}
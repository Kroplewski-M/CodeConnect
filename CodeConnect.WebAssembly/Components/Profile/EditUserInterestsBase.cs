using ApplicationLayer;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using Azure;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class EditUserInterestsBase : ComponentBase
{
    [Inject] public required IUserService UserService { get; set; }
    [Parameter] public EventCallback Cancel { get; set; } 
    [Parameter] public List<TechInterestsDto>? CurrentUserInterests { get; set; }
    [CascadingParameter] public required string Username { get; set; }
    [Inject] public required ToastService ToastService { get; set; }

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
            ToastService.PushToast(
                new ApplicationLayer.Toast("Failed to fetch interests", ToastType.Error));
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
            ToastService.PushToast(
                new ApplicationLayer.Toast("Updating interests", ToastType.Info));
            if (CurrentUserInterests != null)
            {
                var result = await UserService.UpdateUserInterests(new UpdateTechInterestsDto(Username, CurrentUserInterests));
                if (result.Flag)
                {
                    ToastService.PushToast(
                        new ApplicationLayer.Toast(result.Message, ToastType.Success));
                    return;
                }

                ToastService.PushToast(
                    new ApplicationLayer.Toast(result.Message, ToastType.Error));
            }
        }
        catch
        {
            ToastService.PushToast(
                new ApplicationLayer.Toast("An error occured please try again later.", ToastType.Error));
        }
        finally
        {
            Saving = false;
            await Cancel.InvokeAsync(null);
            StateHasChanged();
        }
    }
}
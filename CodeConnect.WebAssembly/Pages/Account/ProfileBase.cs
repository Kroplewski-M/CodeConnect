using ApplicationLayer.Classes;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using CodeConnect.WebAssembly.Components;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CodeConnect.WebAssembly.Pages.Account;

public class ProfileBase : ComponentBase
{
    [Inject] public required  IAuthenticateServiceClient AuthenticateServiceClient { get; set; }
    [Inject] public required IUserService UserService { get; set; }
    [Inject] public required IFollowingService FollowingService { get; set; }
    [Parameter] public string? Username { get; set; }
    protected bool ShowConfirmLogout = false;
    protected bool ShowEditProfile = false;
    protected bool IsCurrentUser = false;
    protected bool FoundUser = false;
    protected UserDetails? UserDetails = null;
    protected FollowerCount? FollowerCount = null;
    protected List<TechInterestsDto>? UserInterests { get; set; }
    [CascadingParameter] public required UserState UserState { get; set; }
    private string PrevUsername { get; set; } = "";
    private bool LoadingDetails = false;
    protected override async Task OnParametersSetAsync()
    { 
        if(PrevUsername == Username || LoadingDetails || UserState.Current == null || string.IsNullOrWhiteSpace(Username))
            return;
        FoundUser = false;
        PrevUsername = Username ?? "";
        LoadingDetails = true;
        if (UserState?.Current?.UserName == Username)
        {
            IsCurrentUser = true;
            UserDetails = UserState?.Current;
        }
        else
        {
            IsCurrentUser = false;
            UserDetails = await UserService.GetUserDetails(Username ?? "");
        }

        if (UserDetails != null)
        {
            var interests = await UserService.GetUserInterests(UserDetails.UserName);
            UserInterests = interests.Interests;
            FollowerCount = await FollowingService.GetUserFollowersCount(UserDetails.UserName);
            FoundUser = true;
        }
        LoadingDetails = false;
    }
    protected void ToggleShowConfirmLogout()
    {
        ShowConfirmLogout = !ShowConfirmLogout;
        StateHasChanged();
    }
    protected void ToggleEditProfile()
    {
        ShowEditProfile = !ShowEditProfile;
        StateHasChanged();
    }
    public Consts.ImageType UpdateImageType { get; set; }
    protected bool ShowUpdateImage { get; set; }
    protected void UpdateImage(Consts.ImageType imageImageType)
    {
        UpdateImageType = imageImageType;
        ShowUpdateImage = true;
        StateHasChanged();
    }

    protected void ToggleEditImage()
    {
        ShowUpdateImage = !ShowUpdateImage;
    }

    protected bool EditUserInterests = false;

    protected void ToggleEditUserInterests()
    {
        EditUserInterests = !EditUserInterests;
    }
}
using DomainLayer.Constants;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class ProfileDetailsBase : ComponentBase
{
    public Constants.ProfileDetailsView ProfileDetailsView { get; set; } = Constants.ProfileDetailsView.Posts;

    public void SetProfileDetails(Constants.ProfileDetailsView option)
    {
        ProfileDetailsView = option;
        StateHasChanged();
    }
    
}
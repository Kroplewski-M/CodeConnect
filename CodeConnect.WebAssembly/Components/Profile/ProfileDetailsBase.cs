using DomainLayer.Constants;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class ProfileDetailsBase : ComponentBase
{
    public Consts.ProfileDetailsView ProfileDetailsView { get; set; } = Consts.ProfileDetailsView.Posts;
    

    public void SetProfileDetails(Consts.ProfileDetailsView option)
    {
        ProfileDetailsView = option;
        StateHasChanged();
    }
    
}
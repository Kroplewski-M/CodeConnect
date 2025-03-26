using DomainLayer.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace CodeConnect.WebAssembly.Components.Profile;

public class ProfileDetailsBase : ComponentBase
{
    protected Consts.ProfileDetailsView ProfileDetailsView { get; set; } = Consts.ProfileDetailsView.Posts;
    [Inject]
     NavigationManager NavigationManager { get; set; } = null!;
    protected override void OnInitialized()
    {
        base.OnInitialized();
        NavigationManager.LocationChanged += OnLocationChanged;
    }
    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        ProfileDetailsView = Consts.ProfileDetailsView.Posts;
        StateHasChanged();
    }
    public void SetProfileDetails(Consts.ProfileDetailsView option)
    {
        ProfileDetailsView = option;
        StateHasChanged();
    }
}
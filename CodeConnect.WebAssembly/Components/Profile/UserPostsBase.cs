using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class UserPostsBase : ComponentBase
{
    [Inject] public required IPostService PostService { get; set; }
    [CascadingParameter] public required string Username { get; set; }
    
    public List<PostBasicDto> Posts { get; set; } = new List<PostBasicDto>();
    public bool Loading { get; set; } = true;

    protected override async Task OnParametersSetAsync()
    {
        Loading = true;
        Posts = await PostService.GetUserPosts(Username);
        Loading = false;
        StateHasChanged();
    }
}
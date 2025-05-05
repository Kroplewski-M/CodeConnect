using ApplicationLayer.APIServices;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class UserPostsBase : ComponentBase
{
    [Inject] public required IPostService PostService { get; set; }
    [CascadingParameter] public required string Username { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        var posts = await PostService.GetUserPosts(Username);
    }
}
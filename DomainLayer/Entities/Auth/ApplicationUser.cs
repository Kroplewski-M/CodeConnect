using Microsoft.AspNetCore.Identity;

namespace DomainLayer.Entities.Auth;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImage { get; set; }
    public string? BackgroundImage { get; set; }
    public string? GithubLink { get; set; }
    public string? WebsiteLink { get; set; }
    public DateOnly? DOB { get; set; }
    public DateOnly CreatedAt { get; set; }
}
using DomainLayer.Constants;
using FluentValidation;

namespace DomainLayer.Entities;

public class EditProfileForm
{
    public string UserId { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; }= "";
    public DateOnly? Dob { get; set; } = null;
    public string Bio { get; set; } = "";
    public string GithubLink { get; set; } = "";
    public string WebsiteLink { get; set; } = "";

}
public class EditProfileValidator: AbstractValidator<EditProfileForm>
{
    public EditProfileValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Dob).LessThan(x=> DateOnly.FromDateTime(DateTime.UtcNow)).NotEmpty();
        RuleFor(x => x.GithubLink)
            .Must(x => string.IsNullOrEmpty(x) || x.StartsWith(Consts.GitHubEndpoint))
            .WithMessage("Github link must start with 'https://github.com/'");
        RuleFor(x => x.WebsiteLink)
            .Must(x => string.IsNullOrEmpty(x) || x.StartsWith(Consts.WebLink))
            .WithMessage("Website link must start with 'https://'");
        RuleFor(x => x.Bio).MaximumLength(500);
    }
}
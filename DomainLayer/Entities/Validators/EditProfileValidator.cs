using DomainLayer.Entities.Auth;
using FluentValidation;

namespace DomainLayer.Entities.Validators;

public class EditProfileValidator: AbstractValidator<EditProfileForm>
{
    public EditProfileValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.DOB).NotEmpty();
        RuleFor(x => x.GithubLink)
            .Must(x => string.IsNullOrEmpty(x) || x.StartsWith("https://github.com/"))
            .WithMessage("Github link must start with 'https://github.com/'");
        RuleFor(x => x.WebsiteLink)
            .Must(x => string.IsNullOrEmpty(x) || x.StartsWith("https://"))
            .WithMessage("Website link must start with 'https://'");
        RuleFor(x => x.Bio).MaximumLength(500);
    }
}
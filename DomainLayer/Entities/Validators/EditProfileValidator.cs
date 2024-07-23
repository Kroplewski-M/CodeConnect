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
    }
}
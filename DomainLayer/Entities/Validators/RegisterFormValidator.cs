using DomainLayer.Entities.Auth;
using FluentValidation;
using FluentValidation.Validators;

namespace DomainLayer.Entities.Validators;

public class RegisterFormValidator : AbstractValidator<RegisterForm>
{
    public RegisterFormValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Dob).NotEmpty();
        RuleFor(x=> x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.UserName).NotEmpty().MinimumLength(3).Matches(@"^\S*$").WithMessage("The property cannot contain spaces.");;
    }
}
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
        RuleFor(x=> x.Password).NotEmpty().MinimumLength(8);
    }
}
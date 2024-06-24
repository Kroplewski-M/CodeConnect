using DomainLayer.Entities.Auth;
using FluentValidation;

namespace DomainLayer.Entities.Validators;

public class LoginFormValidator : AbstractValidator<LoginForm>
{
    public LoginFormValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
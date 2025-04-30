using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace DomainLayer.Entities.Auth;

public class RegisterForm
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateOnly Dob { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string Password { get; set; } = string.Empty;
}
public class RegisterFormValidator : AbstractValidator<RegisterForm>
{
    public RegisterFormValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Dob).LessThan(x=> DateOnly.FromDateTime(DateTime.UtcNow)).NotEmpty();
        RuleFor(x=> x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.UserName).NotEmpty().MinimumLength(3).Matches(@"^\S*$").WithMessage("The property cannot contain spaces.");;
    }
}
using System.ComponentModel.DataAnnotations;
using System.Data;
using FluentValidation;

namespace DomainLayer.Entities.Auth;

public class LoginForm
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";

}
public class LoginFormValidator : AbstractValidator<LoginForm>
{
    public LoginFormValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
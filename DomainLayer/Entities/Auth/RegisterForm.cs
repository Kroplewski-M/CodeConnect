using System.ComponentModel.DataAnnotations;

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
using System.ComponentModel.DataAnnotations;

namespace DomainLayer.Entities.Auth;

public class RegisterForm
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DOB { get; set; } = DateTime.Now;
    public string Password { get; set; } = string.Empty;
}
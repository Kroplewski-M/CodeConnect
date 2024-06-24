using System.ComponentModel.DataAnnotations;

namespace DomainLayer.Entities.Auth;

public class RegisterForm
{
    [Required] 
    public string FirstName { get; set; } = string.Empty;
    [Required] 
    public string LastName { get; set; } = string.Empty;
    [Required] 
    public string Email { get; set; } = string.Empty;
    [Required]
    public DateTime DOB { get; set; } = DateTime.Now;

    [Required] 
    [DataType(DataType.Password)]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
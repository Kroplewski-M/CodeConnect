using System.ComponentModel.DataAnnotations;
using System.Data;

namespace DomainLayer.Entities.Auth;

public class LoginFormViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";
    [DataType(DataType.Password)]
    [Required]
    public string Password { get; set; } = "";

}
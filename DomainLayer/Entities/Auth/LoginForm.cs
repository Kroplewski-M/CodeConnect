using System.ComponentModel.DataAnnotations;
using System.Data;

namespace DomainLayer.Entities.Auth;

public class LoginForm
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";

}
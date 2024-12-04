namespace DomainLayer.Entities.Auth;

public class RefreshUserAuth
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string RefreshToken { get; set; }
}
namespace DomainLayer.Entities.APIClasses;

public class JwtSettings
{
    public string Issuer { get; set; } = "";
    public string Key { get; set; } = "";
    public string Audience { get; set; } = "";
}
using DomainLayer.Entities.Auth;

namespace DomainLayer.DbEnts;

public class UserInterests
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public int TechInterestId { get; set; }
    public TechInterests? TechInterest { get; set; }
}
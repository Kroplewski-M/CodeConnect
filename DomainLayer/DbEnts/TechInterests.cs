namespace DomainLayer.DbEnts;

public class TechInterests
{
    public int Id { get; set; }
    public int InterestId { get; set; }
    public Interest? Interest { get; set; }
    public string Name { get; set; } = "";
}
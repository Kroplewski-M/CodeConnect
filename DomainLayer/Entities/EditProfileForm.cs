namespace DomainLayer.Entities;

public class EditProfileForm
{
    public string UserID { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; }= "";
    public DateOnly? DOB { get; set; } = null;
    public string Bio { get; set; } = "";
    public string GithubLink { get; set; } = "";
    public string WebsiteLink { get; set; } = "";

}

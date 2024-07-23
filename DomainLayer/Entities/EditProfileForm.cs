namespace DomainLayer.Entities;

public class EditProfileForm
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly DOB { get; set; }
    public string ProfileImgUrl { get; set; }
    public string ProfileImgExtention { get; set; }
    public string BackgroundImgUrl { get; set; }
    public string BackgoundImgExtention { get; set; }
}
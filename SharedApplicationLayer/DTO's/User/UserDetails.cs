namespace ApplicationLayer.DTO_s.User;

public record UserDetails(string FirstName,string LastName, string UserName,string Email,string ProfileImg,string BackgroundImg, string GithubLink,string WebsiteLink, DateOnly? Dob,DateOnly? CreatedAt, string Bio);
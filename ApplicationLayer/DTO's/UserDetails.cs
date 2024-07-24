namespace ApplicationLayer.DTO_s;

public record UserDetails(string firstName,string lastName, string email,string profileImg,string BackgroundImg, string githubLink,string websiteLink, DateOnly DOB, string bio);
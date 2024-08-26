namespace DomainLayer.Constants;

public static class Constants
{
    public static class Base
    {
        public const string baseUrl = "https://localhost:7124";
    }
    public static class Tokens
    {
        public const string AuthToken = "AuthToken";
        public const string AuthType = "Jwt";
        public const string ApiAuthTokenName = "Bearer";
    }
    public static class ClaimTypes
    {
        public const string FirstName = "FirstName";
        public const string LastName = "LastName";
        public const string Email = "Email";
        public const string ProfileImg = "ProfileImg";
        public const string BackgroundImg = "BackgroundImg";
        public const string GithubLink = "GithubLink";
        public const string WebsiteLink = "WebsiteLink";
        public const string Dob = "DOB";
        public const string Bio = "Bio";
        public const string CreatedAt = "CreatedAt";
        public const string UserName = "UserName";
        public const string Password = "Password";
    }

    public static class ProfilleDefaults
    {
        public const string ProfileImg = "images/profileImg.jpg";
        public const string BackgroundImg = "images/background.jpg";
        
    }
}
namespace DomainLayer.Constants;

public static class Constants
{
    public static class Base
    {
        public const string BaseUrl = "https://localhost:7124";
        public const int MaxFileSize = 10 * 1024 * 1024; //10MB
    }
    public static class Tokens
    {
        public const string AuthToken = "AuthToken";
        public const string RefreshToken = "RefreshToken";
        public const string AuthType = "Jwt";
        public const string ApiAuthTokenName = "Bearer";
        public const int AuthTokenMins = 30;
        public const int RefreshTokenMins = 10080;
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

    public static class ProfileDefaults
    {
        public const string ProfileImg = "images/profileImg.jpg";
        public const string BackgroundImg = "images/background.jpg";
    }

    public static class CacheKeys
    {
        public const string AllInterests = "9ebeec5c-ba6d-4b03-AllInterests-4a94e2e2c15a";
    }
    public enum ImageTypeOfUpdate
    {
        ProfileImage,
        BackgroundImage
    }

    public enum ProfileDetailsView
    {
        Posts,
        Followers,
        Following
    }
}
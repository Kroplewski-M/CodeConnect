namespace DomainLayer.Constants;

public static class Consts
{
    public static class SignalR
    {
        public const string HubName = "notifications";
        public const string NotificationMethodWatch = "NotificationPing";
    }
    public static class AuthEndpoints
    {
        public static readonly List<string> AuthUrls = [RegisterEndpoint,LoginEndpoint];
        private const string RegisterEndpoint = "/api/Authentication/RegisterUser";
        private const string LoginEndpoint = "/api/Authentication/LoginUser";
    }
    
    public static class Base
    {
        public const string BaseUrl = "https://localhost:7124";
        public const int MaxFileSize = 10 * 1024 * 1024; //10MB
        public const int UploadMaxFileSize = MaxFileSize / 2; //5MB

        public const string DateFormat = "MM/dd/yyyy";
    }
    public static class Tokens
    {
        public const string AuthToken = "AuthToken";
        public const string RefreshToken = "RefreshToken";
        public const string AuthType = "Jwt";
        public const string ApiAuthTokenName = "Bearer";
        public const int AuthTokenMins = 10;
        public const int RefreshTokenMins = 10080;
        public const string TokenType = "Token";
    }

    public static class Headers
    {
        public const string DeviceId = "X-Device-ID";
    }
    public enum TokenType
    {
        Access,
        Refresh,
    }
    public static class ClaimTypes
    {
        public const string Id = System.Security.Claims.ClaimTypes.NameIdentifier;
        public const string FirstName = System.Security.Claims.ClaimTypes.Name;
        public const string LastName = System.Security.Claims.ClaimTypes.Surname;
        public const string Email = System.Security.Claims.ClaimTypes.Email;
        public const string Dob = "Dob";
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
    public enum ImageType
    {
        ProfileImages,
        BackgroundImages,
        PostImages,
    }
    public enum ProfileDetailsView
    {
        Posts,
        Followers,
        Following
    }

    public static class DateFormats
    {
        public const string DateFormat = "MM/dd/yyyy";
        public const string DateTimeFormat = "MM/dd/yyyy H:mm";
    }
    public static readonly string AzureBlobEndpoint = "https://codeconnectblobs.blob.core.windows.net";
    public static readonly string GitHubEndpoint = "https://github.com";
    public static readonly string WebLink = "https://";
}
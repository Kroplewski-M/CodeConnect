using DomainLayer.Constants;

namespace DomainLayer.Helpers;

public static class Helpers
{
    public static string GetAzureImgUrl(Consts.ImageType imgType ,string? imgName)
    { 
        if (string.IsNullOrWhiteSpace(imgName))
            return "";
        return $"{Consts.AzureBlobEndpoint}/{imgType.ToString().ToLower()}/{imgName}";
    }
    public static bool IsBase64(string base64)
    {   
        if (string.IsNullOrWhiteSpace(base64))
            return false;
        Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer , out int bytesParsed);
    }

    public static string GetUserImgUrl(string? userImg, Consts.ImageType imgType)
    {
        if (string.IsNullOrWhiteSpace(userImg))
        {
            if (imgType == Consts.ImageType.ProfileImages)
                return Consts.ProfileDefaults.ProfileImg;
            if(imgType == Consts.ImageType.BackgroundImages)
                return Consts.ProfileDefaults.BackgroundImg;
        }
        return GetAzureImgUrl(imgType, userImg);
    }

    public static string GetUsersLocalTime(DateTime dateTime, string format = Consts.DateFormats.DateFormat)
    {
        return dateTime.ToLocalTime().ToString(format);
    }
    public static double BytesToMegabytes(long bytes)
    {
        return bytes / 1048576.0; // 1024 * 1024
    }
    public static string GetMessage(this Consts.NotificationTypes type)
    {
        return type switch
        {
            Consts.NotificationTypes.CommentLike => "liked your comment",
            Consts.NotificationTypes.CommentReply => "replied to your comment",
            Consts.NotificationTypes.PostLike => "liked your post",
            Consts.NotificationTypes.PostComment => "commented on your post",
            Consts.NotificationTypes.Follow => "started following you",
            _ => ""
        };
    }
}
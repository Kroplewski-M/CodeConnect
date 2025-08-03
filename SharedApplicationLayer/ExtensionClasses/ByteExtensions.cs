using ImageMagick;

namespace ApplicationLayer.ExtensionClasses;

public static class ByteExtensions
{
    public static byte[] RemoveMetaData(this byte[] bytes)
    {
        var image = new MagickImage(bytes);
        // Exif profile: Camera settings, GPS location, Date, Time Zone, ... etc
        var exifProfile = image.GetExifProfile(); 
        // IPTC profile: Copyright, Permissions and licenses, Creator's information and contact details, Rights usage terms, ...etc
        var iptcProfile  = image.GetIptcProfile(); 
        // Xmp profile: IPTC with additional information
        var xmpProfile = image.GetXmpProfile();
        
        if(exifProfile != null) image.RemoveProfile(exifProfile);
        if(iptcProfile != null) image.RemoveProfile(iptcProfile);
        if(xmpProfile != null) image.RemoveProfile(xmpProfile);
        return image.ToByteArray();
    }
}
using System.Security.Cryptography;
using System.Text;

namespace comicsbox.Services;

public class ThumbnailHelper
{
    public static string GetThumbnailFileName(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var serie = Path.GetFileName(Path.GetDirectoryName(filePath));
        var type = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(filePath)));

        var thumbnailFileName = $"{type}_{serie}_{fileName}".ToLower();
        var hexString = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(thumbnailFileName)));

        return $"{hexString}.jpg";
    }
}
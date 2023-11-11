using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Comicsbox.Imaging
{
    public static class FileInfoExtension
    {
        public static byte[] ToByteArray(this IFileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return new byte[0];
            }

            using (Stream input = fileInfo.CreateReadStream())
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}

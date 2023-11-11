using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Comicsbox.Imaging
{
    public static class ImageExtension
    {
        public static byte[] ToByteArray(this Bitmap image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
    }
}

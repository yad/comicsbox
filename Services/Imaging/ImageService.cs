using SkiaSharp;

namespace Comicsbox
{
    public class ImageService
    {
        public Stream ScaleAsThumbnail(byte[] bytes)
        {
            using (SKImage image = SKImage.FromEncodedData(bytes))
            {
                int sourceWidth = image.Width;
                int sourceHeight = image.Height;

                float dpi = 72.0f;
                float ratio = 2.54f / dpi;
                int maxside = (int)(5.2f / ratio);

                int destHeight = maxside;
                int destWidth = destHeight * sourceWidth / sourceHeight;

                using (var surface = SKSurface.Create(destWidth, destHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul))
                using (var paint = new SKPaint())
                {
                    // high quality with antialiasing
                    paint.IsAntialias = true;
                    paint.FilterQuality = SKFilterQuality.High;

                    // draw the bitmap to fill the surface
                    surface.Canvas.DrawImage(image, new SKRectI(0, 0, destWidth, destHeight), paint);
                    surface.Canvas.Flush();

                    using (var newImg = surface.Snapshot())
                    {
                        return newImg.Encode(SKEncodedImageFormat.Jpeg, 100).AsStream();
                    }
                }
            }


            // using (MemoryStream image = new MemoryStream(bytes))
            // using (var fullBitmap = Image.FromStream(image))
            // {
            //     int sourceWidth = fullBitmap.Width;
            //     int sourceHeight = fullBitmap.Height;
            //     int sourceX = 0;
            //     int sourceY = 0;

            //     int destX = 0;
            //     int destY = 0;

            //     float dpi = 72.0f;
            //     float ratio = 2.54f / dpi;
            //     int maxside = (int)(5.2f / ratio);

            //     int destHeight = maxside;
            //     int destWidth = destHeight * sourceWidth / sourceHeight;

            //     return Resize(fullBitmap, sourceWidth, sourceHeight, sourceX, sourceY, destWidth, destHeight, destX, destY, dpi);
            // }
        }

        // private static byte[] Resize(Image fullBitmap, int sourceWidth, int sourceHeight, int sourceX, int sourceY, int destWidth, int destHeight, int destX, int destY, float dpi)
        // {
        //     byte[] result;
        //     using (Bitmap thumbnailBitmap = new Bitmap(destWidth, destHeight, PixelFormat.Format16bppRgb565))
        //     {
        //         thumbnailBitmap.SetResolution(dpi, dpi);

        //         using (var graphics = Graphics.FromImage(thumbnailBitmap))
        //         {
        //             graphics.SmoothingMode = SmoothingMode.HighQuality;
        //             graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //             graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        //             graphics.DrawImage(fullBitmap,
        //                 new Rectangle(destX, destY, destWidth, destHeight),
        //                 new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
        //                 GraphicsUnit.Pixel);

        //             result = thumbnailBitmap.ToByteArray();
        //         }
        //     }

        //     return result;
        // }

        // public byte[] TakePageSide(byte[] bytes, ImageSide imageSide)
        // {
        //     if (imageSide == ImageSide.Both)
        //     {
        //         return bytes;
        //     }

        //     using (MemoryStream image = new MemoryStream(bytes))
        //     using (var fullBitmap = Image.FromStream(image))
        //     {
        //         int sourceWidth = fullBitmap.Width / 2;
        //         int sourceHeight = fullBitmap.Height;
        //         int sourceX = imageSide == ImageSide.Left ? 0 : sourceWidth;
        //         int sourceY = 0;

        //         int destX = 0;
        //         int destY = 0;
        //         int destWidth = sourceWidth;
        //         int destHeight = sourceHeight;

        //         return Resize(fullBitmap, sourceWidth, sourceHeight, sourceX, sourceY, destWidth, destHeight, destX, destY, fullBitmap.HorizontalResolution);
        //     }
        // }
    }
}

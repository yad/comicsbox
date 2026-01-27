using SkiaSharp;

namespace comicsbox.Services;

public class ImageService
{
    public byte[] ScaleAsThumbnail(byte[] bytes)
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

            var info = new SKImageInfo(destWidth, destHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(info))
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
                    return newImg.Encode(SKEncodedImageFormat.Jpeg, 100).ToArray();
                }
            }
        }
    }

    public byte[] TakePageSide(byte[] bytes, ImageSide imageSide)
    {
        if (imageSide == ImageSide.Both)
        {
            return bytes;
        }

        using (SKImage image = SKImage.FromEncodedData(bytes))
        {
            int sourceWidth = image.Width / 2;
            int sourceHeight = image.Height;
            int sourceX = imageSide == ImageSide.Left ? 0 : sourceWidth;

            int destWidth = sourceWidth;
            int destHeight = sourceHeight;

            var info = new SKImageInfo(destWidth, destHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(info))
            using (var paint = new SKPaint())
            {
                // high quality with antialiasing
                paint.IsAntialias = true;
                paint.FilterQuality = SKFilterQuality.High;

                // draw the bitmap to fill the surface
                surface.Canvas.DrawImage(image, new SKRectI(sourceX, 0, sourceX + destWidth, destHeight), new SKRectI(0, 0, destWidth, destHeight), paint);
                surface.Canvas.Flush();

                using (var newImg = surface.Snapshot())
                {
                    return newImg.Encode(SKEncodedImageFormat.Jpeg, 100).ToArray();
                }
            }
        }
    }
}

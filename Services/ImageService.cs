using SkiaSharp;

namespace comicsbox.Services;

public class ImageService
{
    public byte[] ScaleAsThumbnail(byte[] bytes)
    {
        using var input = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(input);

        if (codec == null)
            throw new InvalidOperationException("Invalid image data");

        var originalInfo = codec.Info;
        using var bitmap = new SKBitmap(originalInfo);
        codec.GetPixels(bitmap.Info, bitmap.GetPixels());

        int sourceWidth = bitmap.Width;
        int sourceHeight = bitmap.Height;

        // 🎯 Taille cible WEB (pas impression)
        const int targetHeight = 320; // parfait pour cartes
        int targetWidth = sourceWidth * targetHeight / sourceHeight;

        var resizedInfo = new SKImageInfo(
            targetWidth,
            targetHeight,
            SKColorType.Rgb565, // 👈 énorme gain de poids
            SKAlphaType.Opaque
        );

        using var resizedBitmap = new SKBitmap(resizedInfo);

        var sampling = new SKSamplingOptions(
            SKFilterMode.Linear,
            SKMipmapMode.None
        );

        bitmap.ScalePixels(resizedBitmap, sampling);

        using var image = SKImage.FromBitmap(resizedBitmap);

        // 🎯 Qualité JPEG calibrée
        using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 78);

        return encoded.ToArray();
    }


    public byte[] TakePageSide(byte[] bytes, ImageSide imageSide)
    {
        if (imageSide == ImageSide.Both)
            return bytes;

        using var input = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(input);

        if (codec == null)
            throw new InvalidOperationException("Invalid image data");

        var info = codec.Info;
        using var bitmap = new SKBitmap(info);
        codec.GetPixels(bitmap.Info, bitmap.GetPixels());

        int halfWidth = bitmap.Width / 2;
        int height = bitmap.Height;

        var subsetRect = imageSide == ImageSide.Left
            ? new SKRectI(0, 0, halfWidth, height)
            : new SKRectI(halfWidth, 0, bitmap.Width, height);

        using var sideBitmap = new SKBitmap(
            new SKImageInfo(halfWidth, height, bitmap.ColorType, bitmap.AlphaType)
        );

        if (!bitmap.ExtractSubset(sideBitmap, subsetRect))
            throw new InvalidOperationException("Failed to extract page side");

        using var image = SKImage.FromBitmap(sideBitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 90);

        return encoded.ToArray();
    }
}

using System.Security.Cryptography;
using System.Text;

namespace Comicsbox
{
    public class ThumbnailProvider
    {
        private readonly ImageService _imageService;

        public ThumbnailProvider(ImageService imageService)
        {
            _imageService = imageService;
        }

        public string GetThumbnailFileName(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var serie = Path.GetFileName(Path.GetDirectoryName(filePath));
            var type = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(filePath)));

            var thumbnailFileName = $"{type}_{serie}_{fileName}".ToLower();

            var hexString = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(thumbnailFileName)));

            return $"{hexString}.jpg";
        }

        public void ProcessFile(string filePath)
        {
            var cacheThumbnail = Path.Combine("wwwroot", "cache", "thumbnails", GetThumbnailFileName(filePath));
            if (File.Exists(cacheThumbnail))
            {
                return;
            }

            try
            {
                var pdfReaderService = new PdfReaderService();
                var image = pdfReaderService.ReadCoverImage(filePath);

                using (var thumbnailContent = _imageService.ScaleAsThumbnail(image))
                using (var fileStream = File.Create(cacheThumbnail))
                {
                    thumbnailContent.Seek(0, SeekOrigin.Begin);
                    thumbnailContent.CopyTo(fileStream);
                }

                // Console.WriteLine($"{filePath}:: DONE ({cacheThumbnail})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{filePath}:: {ex}");
            }
        }
    }
}

using System.Security.Cryptography;
using System.Text;

namespace Comicsbox
{
    public class ThumbnailProvider
    {
        private readonly ImageService _imageService;
        private readonly PdfReaderService _pdfReaderService;

        public ThumbnailProvider(ImageService imageService, PdfReaderService pdfReaderService)
        {
            _imageService = imageService;
            _pdfReaderService = pdfReaderService;
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

        public void ProcessFile(FileInfo fileInfo, bool isReversed)
        {
            var cacheThumbnail = Path.Combine("wwwroot", "cache", "thumbnails", GetThumbnailFileName(fileInfo.FullName));

            if (File.Exists(cacheThumbnail))
            {
                var dst = new FileInfo(cacheThumbnail);

                if (dst.LastWriteTimeUtc < fileInfo.LastWriteTimeUtc)
                {
                    File.Delete(cacheThumbnail);
                }
                else
                {
                    return;
                }
            }

            try
            {
                var image = _pdfReaderService.LoadFile(fileInfo.FullName, isReversed).ReadCoverImage();
                var thumbnailContent = _imageService.ScaleAsThumbnail(image);
                File.WriteAllBytes(cacheThumbnail, thumbnailContent);
                // Console.WriteLine($"{filePath}:: DONE ({cacheThumbnail})");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"{fileInfo.FullName}:: Thumbnail cannot be found");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fileInfo.FullName}:: {ex}");
            }
        }
    }
}

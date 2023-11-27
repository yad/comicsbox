using iText.Kernel.Pdf;

namespace Comicsbox
{
    public class PdfReaderService : IDisposable
    {
        private readonly ImageService _imageService;

        private PdfReader _pdfReader = null!;
        private PdfDocument _pdfDocument = null!;
        private bool _isReversed = false;

        public PdfReaderService(ImageService imageService)
        {
            _imageService = imageService;
        }

        public PdfReaderService LoadFile(string pdfPath, bool isReversed)
        {
            _pdfReader = new PdfReader(pdfPath);
            _pdfDocument = new PdfDocument(_pdfReader);
            _isReversed = isReversed;
            return this;
        }

        public byte[] ReadCoverImage()
        {
            int firstPage = IsDoublePage(1) ? 2 : 1;
            return GetPageN(firstPage);
        }

        public void Extract(string path, int page)
        {
            var lastPageNumber = GetLastPageNumber();

            // fast extract current page
            if (page <= lastPageNumber)
            {
                var filePath = Path.Combine(path, $"{page}.jpg");
                if (!File.Exists(filePath))
                {
                    var pageContent = GetPageN(page);
                    File.WriteAllBytes(filePath, pageContent);
                }
            }

            // extract all pages
            for (int i = 1; i <= lastPageNumber; i++)
            {
                var filePath = Path.Combine(path, $"{i}.jpg");
                if (!File.Exists(filePath))
                {
                    var pageContent = GetPageN(i);
                    File.WriteAllBytes(filePath, pageContent);
                }
            }

            // finished create flag
            var eofPath = Path.Combine(path, $"{lastPageNumber + 1}.eof");
            File.WriteAllText(eofPath, "");
        }

        private byte[] GetPageN(int page)
        {
            int actualPageIndex = 1;
            int virtualPageIndex = 0;
            ImageSide side = ImageSide.Both;

            for (; actualPageIndex <= _pdfDocument.GetNumberOfPages(); actualPageIndex++)
            {
                var isDoublePage = IsDoublePage(actualPageIndex);

                virtualPageIndex++;
                if (page == virtualPageIndex)
                {
                    side = isDoublePage ? (_isReversed ? ImageSide.Right : ImageSide.Left) : ImageSide.Both;
                    break;
                }

                if (isDoublePage)
                {
                    virtualPageIndex++;
                    if (page == virtualPageIndex)
                    {
                        side = ImageSide.Right;
                        break;
                    }
                }
            }

            var resources = _pdfDocument.GetPage(actualPageIndex).GetResources();
            foreach (var resource in resources.GetResourceNames())
            {
                var pdfImage = resources.GetImage(resource);
                if (pdfImage != null)
                {
                    var imageBytes = pdfImage.GetImageBytes();
                    return _imageService.TakePageSide(imageBytes, side);
                }
            }

            throw new InvalidOperationException($"Current PDF file is invalid");
        }

        private bool IsDoublePage(int page)
        {
            var currentPage = _pdfDocument.GetPage(page).GetPageSize();

            var height = currentPage.GetHeight();
            var width = currentPage.GetWidth();

            if (height > width)
            {
                // is portrait
                return false;
            }

            var diff = Math.Abs((height - width) * 100 / height);
            if (diff < 10)
            {
                // is square
                return false;
            }

            // landscape
            return true;
        }

        public void Dispose()
        {
            ((IDisposable)_pdfDocument)?.Dispose();
            ((IDisposable)_pdfReader)?.Dispose();
        }

        public int GetLastPageNumber()
        {
            int lastPageNumber = 0;
            for (int actualPageIndex = 1; actualPageIndex <= _pdfDocument.GetNumberOfPages(); actualPageIndex++)
            {
                lastPageNumber++;

                if (IsDoublePage(actualPageIndex))
                {
                    lastPageNumber++;
                }
            }

            return lastPageNumber;
        }
    }
}

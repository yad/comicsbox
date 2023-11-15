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

        // public bool IsPageExists(int page)
        // {
        //     return GetPageN(page) != null;
        // }

        private byte[] GetPageN(int page)
        {
            int actualPageIndex = 1;
            int virtualPageIndex = 0;
            ImageSide side = ImageSide.Both;

            for ( ; actualPageIndex <= _pdfDocument.GetNumberOfPages(); actualPageIndex++)
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

            var resources = _pdfDocument.GetPage(1).GetResources();
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

        // public int GetLastPageNumber()
        // {
        //     int lastPageNumber = 0;
        //     for (int actualPageIndex = 1; actualPageIndex <= _pdfReader.NumberOfPages; actualPageIndex++)
        //     {
        //         lastPageNumber++;

        //         if (IsDoublePage(actualPageIndex))
        //         {
        //             lastPageNumber++;
        //         }
        //     }

        //     return lastPageNumber;
        // }
    }
}

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

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
            return GetPageN(1);
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
                    if (!isDoublePage)
                    {
                        side = ImageSide.Both;
                    }
                    else if (page == 1)
                    {
                        side = _isReversed ? ImageSide.Left : ImageSide.Right;
                    }
                    else
                    {
                        side = _isReversed ? ImageSide.Right : ImageSide.Left;
                    }
                    break;
                }

                if (isDoublePage)
                {
                    virtualPageIndex++;
                    if (page == virtualPageIndex)
                    {
                        if (page == 2)
                        {
                            side = _isReversed ? ImageSide.Right : ImageSide.Left;
                        }
                        else
                        {
                            side = _isReversed ? ImageSide.Left : ImageSide.Right;
                        }

                        break;
                    }
                }
            }

            var strategy = new ImageRenderListener();
            var parser = new PdfCanvasProcessor(strategy);
            parser.ProcessPageContent(_pdfDocument.GetPage(actualPageIndex));
            var imageBytes = strategy.GetResult();
            if (imageBytes != null)
            {
                return _imageService.TakePageSide(imageBytes, side);
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

using Comicsbox.Imaging;
using iTextSharp.text.pdf;
using InnerPdfReader = iTextSharp.text.pdf.PdfReader;

namespace Comicsbox.PdfReader
{
    public class PdfReaderService : IDisposable
    {
        private readonly InnerPdfReader _pdfReader;

        private readonly IImageService _imageService;

        public PdfReaderService(string filename)
        {
            _pdfReader = new InnerPdfReader(filename);
            _imageService = new ImageService();
        }

        public byte[] ReadCoverImage()
        {
            int firstPage = IsDoublePage(1) ? 2 : 1;
            return ReadImageAtPage(firstPage);
        }

        public byte[] ReadImageAtPage(int page)
        {
            var currentPage = GetPageN(page);
            var resources = (PdfDictionary)InnerPdfReader.GetPdfObject(currentPage.PdfPageContent.Get(PdfName.RESOURCES));
            var xobject = (PdfDictionary)InnerPdfReader.GetPdfObject(resources.Get(PdfName.XOBJECT));
            var pdfName = xobject.Keys.OfType<PdfName>().Single();
            var pdfObject = (PRIndirectReference)xobject.Get(pdfName);
            var stream = (PRStream)_pdfReader.GetPdfObject(pdfObject.Number);
            var imageBytes = InnerPdfReader.GetStreamBytesRaw(stream);
            return _imageService.TakePageSide(imageBytes, currentPage.ImageSide);
        }

        public bool IsPageExists(int page)
        {
            return GetPageN(page) != null;
        }

        private PdfPage GetPageN(int page)
        {
            int virtualPageIndex = 0;
            for (int actualPageIndex = 1; actualPageIndex <= _pdfReader.NumberOfPages; actualPageIndex++)
            {
                bool isDoublePage = IsDoublePage(actualPageIndex);

                virtualPageIndex++;
                if (page == virtualPageIndex)
                {
                    return new PdfPage(_pdfReader.GetPageN(actualPageIndex), isDoublePage ? ImageSide.Left : ImageSide.Both);
                }

                if (isDoublePage)
                {
                    virtualPageIndex++;
                    if (page == virtualPageIndex)
                    {
                        return new PdfPage(_pdfReader.GetPageN(actualPageIndex), ImageSide.Right);
                    }
                }
            }

            return null;
        }

        private bool IsDoublePage(int page)
        {
            var currentPage = _pdfReader.GetPageSize(page);
            return currentPage.Width > currentPage.Height;
        }

        public int GetLastPageNumber()
        {
            int lastPageNumber = 0;
            for (int actualPageIndex = 1; actualPageIndex <= _pdfReader.NumberOfPages; actualPageIndex++)
            {
                lastPageNumber++;

                if (IsDoublePage(actualPageIndex))
                {
                    lastPageNumber++;
                }
            }

            return lastPageNumber;
        }

        public void Dispose()
        {
            _pdfReader.Close();
        }
    }
}

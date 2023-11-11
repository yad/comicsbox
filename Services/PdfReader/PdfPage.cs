﻿using Comicsbox.Imaging;
using iTextSharp.text.pdf;

namespace Comicsbox.PdfReader
{
    public class PdfPage
    {
        public static readonly PdfPage NotFound = null;

        public PdfDictionary PdfPageContent { get; set; }

        public ImageSide ImageSide { get; set; }

        public PdfPage(PdfDictionary pdfPageContent, ImageSide imageSide)
        {
            PdfPageContent = pdfPageContent;
            ImageSide = imageSide;
        }
    }
}

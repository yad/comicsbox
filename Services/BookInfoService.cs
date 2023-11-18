namespace Comicsbox
{
    public class BookInfoService
    {
        private readonly IConfiguration _configuration;
        private readonly ThumbnailProvider _thumbnailProvider;
        public static readonly string DefaultFileContainerExtension = ".pdf";

        public BookInfoService(IConfiguration configuration, ThumbnailProvider thumbnailProvider)
        {
            _configuration = configuration;
            _thumbnailProvider = thumbnailProvider;
        }

        public BookContainer<Book> GetBookList(string category, string serie)
        {
            return BuildBookInfo(category, serie);
        }

        public BookContainer<Book> GetBookThumbnails(string category, string serie)
        {
            return BuildBookInfo(category, serie);
        }

        public string GetSeriePath(string category, string serie)
        {
            var basePath = _configuration.GetValue<string>("Settings:AbsoluteBasePath")!;
            return Path.Combine(basePath, category, serie);
        }

        private BookContainer<Book> BuildBookInfo(string category, string serie)
        {
            var path = GetSeriePath(category, serie);

            List<Book> books = new List<Book>();

            if (string.IsNullOrEmpty(serie))
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    serie = Path.GetFileName(dir);
                    var file = Directory.GetFiles(dir).Order().FirstOrDefault(f => Path.GetExtension(f) == DefaultFileContainerExtension);
                    var thumbnail = "";
                    if (file != null)
                    {
                        thumbnail = _thumbnailProvider.GetThumbnailFileName(file);
                    }

                    books.Add(new Book(serie, thumbnail));
                }
            }
            else
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    var book = Path.GetFileNameWithoutExtension(file);
                    var thumbnail = _thumbnailProvider.GetThumbnailFileName(file);
                    books.Add(new Book(book, thumbnail));
                }
            }

            return new BookContainer<Book>("", books);
        }

        // public PageDetail GetDetail(string category, string book, string chapter, int page)
        // {
        //     _filePathFinder.SetPathContext(category, book, chapter);

        //     using (PdfReaderService pdfReader = new PdfReaderService(_filePathFinder.GetPath().AbsolutePath))
        //     {
        //         var image = pdfReader.ReadImageAtPage(page);

        //         string fileContent = Convert.ToBase64String(image);
        //         return new PageDetail(chapter, page)
        //             .WithContent(fileContent)
        //             .WithPrevious(GetPreviousPageAndChapter(pdfReader, page, chapter))
        //             .WithNext(GetNextPageAndChapter(pdfReader, page, chapter));
        //     }
        // }

        // private PageDetail GetPreviousPageAndChapter(PdfReaderService pdfReader, int page, string chapter)
        // {
        //     PageDetail previousPageAndChapter;
        //     int previousPage = page - 1;

        //     if (pdfReader.IsPageExists(previousPage))
        //     {
        //         previousPageAndChapter = new PageDetail(chapter, previousPage);
        //     }
        //     else
        //     {
        //         var previousChapter = _filePathFinder.GetPreviousFileNameOrDefault(chapter, DefaultFileContainerExtension);
        //         if (previousChapter != null)
        //         {
        //             int previousChapterLastPage = GetPreviousChapterLastPage(previousChapter.PhysicalPath);
        //             previousPageAndChapter = new PageDetail(previousChapter.Name, previousChapterLastPage);
        //         }
        //         else
        //         {
        //             previousPageAndChapter = PageDetail.NotFound;
        //         }
        //     }

        //     return previousPageAndChapter;
        // }

        // private static int GetPreviousChapterLastPage(string previousChapterFullPath)
        // {
        //     using (PdfReaderService pdfReaderPreviousFile = new PdfReaderService(previousChapterFullPath))
        //     {
        //         return pdfReaderPreviousFile.GetLastPageNumber();
        //     }
        // }

        // private PageDetail GetNextPageAndChapter(PdfReaderService pdfReader, int page, string chapter)
        // {
        //     PageDetail nextPageAndChapter;
        //     int nextPage = page + 1;

        //     if (pdfReader.IsPageExists(nextPage))
        //     {
        //         nextPageAndChapter = new PageDetail(chapter, nextPage);
        //     }
        //     else
        //     {
        //         var nextChapter = _filePathFinder.GetNextFileNameOrDefault(chapter, DefaultFileContainerExtension);
        //         if (nextChapter != null)
        //         {
        //             const int nextChapterFirstPage = 1;
        //             nextPageAndChapter = new PageDetail(nextChapter.Name, nextChapterFirstPage);
        //         }
        //         else
        //         {
        //             nextPageAndChapter = PageDetail.NotFound;
        //         }
        //     }

        //     return nextPageAndChapter;
        // }
    }
}

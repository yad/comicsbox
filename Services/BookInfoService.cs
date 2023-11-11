using Comicsbox.FileBrowser;
using Comicsbox.PdfReader;
using Comicsbox.Imaging;
using Comicsbox.Cache;
using Comicsbox.Worker;

namespace Comicsbox
{
    public interface IBookInfoService
    {
        BookContainer<Book> GetBookList(params string[] subpaths);
        BookContainer<Book> GetBookThumbnails(params string[] subpaths);
        PageDetail GetDetail(string category, string book, string chapter, int page);
    }

    public class BookInfoService : IBookInfoService
    {
        private readonly IFilePathFinder _filePathFinder;
        private readonly IImageService _imageService;
        private readonly ICacheService _cacheService;
        private readonly ThumbnailWorker _thumbnailWorker;
        public static readonly string DefaultFileContainerExtension = ".pdf";

        public BookInfoService(IFilePathFinder filePathFinder, IImageService imageService, ICacheService cacheService, ThumbnailWorker thumbnailWorker)
        {
            _filePathFinder = filePathFinder;
            _imageService = imageService;
            _cacheService = cacheService;
            _thumbnailWorker = thumbnailWorker;
        }

        public BookContainer<Book> GetBookList(params string[] subpaths)
        {
            string cacheKey = $"booklist_{string.Join("_", subpaths)}";
            return _cacheService.TryLoadFromCache(cacheKey, () => BuildBookInfo(subpaths, true));
        }

        public BookContainer<Book> GetBookThumbnails(params string[] subpaths)
        {
            string cacheKey = $"bookthumbnails_{string.Join("_", subpaths)}";
            var thumbnailWorkerStatus = _thumbnailWorker.GetStatus();
            bool ignoreThumbnails = thumbnailWorkerStatus.IsInProgress;
            return _cacheService.TryLoadFromCache(cacheKey, () => BuildBookInfo(subpaths, ignoreThumbnails));
        }

        private BookContainer<Book> BuildBookInfo(string[] subpaths, bool ignoreThumbnails)
        {
            _filePathFinder.SetPathContext(subpaths);

            List<Book> books = new List<Book>();
            var dirInfo = _filePathFinder.GetDirectoryContents(ListMode.All);
            foreach (var container in dirInfo.Where(f => f.IsDirectory || DefaultFileContainerExtension.Equals(Path.GetExtension(f.Name))))
            {
                _filePathFinder.SetPathContext(subpaths);
                if (container.IsDirectory)
                {
                    _filePathFinder.AppendPathContext(container.Name);
                }

                string thumbnailContent = string.Empty;
                if (!ignoreThumbnails)
                {
                    var thumbnail = new ThumbnailProvider(_filePathFinder).GetThumbnailContent(container.Name);
                    thumbnailContent = Convert.ToBase64String(thumbnail);
                }

                books.Add(new Book(container.Name, thumbnailContent));
            }

            bool cacheEnable = !ignoreThumbnails;
            return new BookContainer<Book>(string.Empty, books).WithCache(cacheEnable);
        }

        public PageDetail GetDetail(string category, string book, string chapter, int page)
        {
            _filePathFinder.SetPathContext(category, book, chapter);

            using (PdfReaderService pdfReader = new PdfReaderService(_filePathFinder.GetPath().AbsolutePath))
            {
                var image = pdfReader.ReadImageAtPage(page);

                string fileContent = Convert.ToBase64String(image);
                return new PageDetail(chapter, page)
                    .WithContent(fileContent)
                    .WithPrevious(GetPreviousPageAndChapter(pdfReader, page, chapter))
                    .WithNext(GetNextPageAndChapter(pdfReader, page, chapter));
            }
        }

        private PageDetail GetPreviousPageAndChapter(PdfReaderService pdfReader, int page, string chapter)
        {
            PageDetail previousPageAndChapter;
            int previousPage = page - 1;

            if (pdfReader.IsPageExists(previousPage))
            {
                previousPageAndChapter = new PageDetail(chapter, previousPage);
            }
            else
            {
                var previousChapter = _filePathFinder.GetPreviousFileNameOrDefault(chapter, DefaultFileContainerExtension);
                if (previousChapter != null)
                {
                    int previousChapterLastPage = GetPreviousChapterLastPage(previousChapter.PhysicalPath);
                    previousPageAndChapter = new PageDetail(previousChapter.Name, previousChapterLastPage);
                }
                else
                {
                    previousPageAndChapter = PageDetail.NotFound;
                }
            }

            return previousPageAndChapter;
        }

        private static int GetPreviousChapterLastPage(string previousChapterFullPath)
        {
            using (PdfReaderService pdfReaderPreviousFile = new PdfReaderService(previousChapterFullPath))
            {
                return pdfReaderPreviousFile.GetLastPageNumber();
            }
        }

        private PageDetail GetNextPageAndChapter(PdfReaderService pdfReader, int page, string chapter)
        {
            PageDetail nextPageAndChapter;
            int nextPage = page + 1;

            if (pdfReader.IsPageExists(nextPage))
            {
                nextPageAndChapter = new PageDetail(chapter, nextPage);
            }
            else
            {
                var nextChapter = _filePathFinder.GetNextFileNameOrDefault(chapter, DefaultFileContainerExtension);
                if (nextChapter != null)
                {
                    const int nextChapterFirstPage = 1;
                    nextPageAndChapter = new PageDetail(nextChapter.Name, nextChapterFirstPage);
                }
                else
                {
                    nextPageAndChapter = PageDetail.NotFound;
                }
            }

            return nextPageAndChapter;
        }
    }
}

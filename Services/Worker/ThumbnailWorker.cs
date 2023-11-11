using Comicsbox.FileBrowser;
using Comicsbox.Imaging;
using Comicsbox.PdfReader;

namespace Comicsbox.Worker
{
    public class ThumbnailWorker
    {
        private readonly IFilePathFinder _filePathFinder;

        private readonly IImageService _imageService;

        private Task _worker;

        private readonly List<ThumbnailInfo> _inProgress;

        private readonly List<ThumbnailWorkerError> _errors;

        public ThumbnailWorker(IFilePathFinder filePathFinder, IImageService imageService)
        {
            _filePathFinder = filePathFinder;
            _imageService = imageService;

            _inProgress = new List<ThumbnailInfo>();
            _errors = new List<ThumbnailWorkerError>();
        }

        public void Start()
        {
            GetTask();
        }

        public async Task Stop()
        {
            await GetTask();
        }

        private Task GetTask()
        {
            if (_worker == null)
            {
                _worker = Task.Factory.StartNew(() => BrowseAndGenerate());
            }

            return _worker;
        }

        public void BrowseAndGenerate()
        {
            BrowseAndGenerate("");

            foreach(var current in _inProgress)
            {
                ProcessFile(current);
            }
        }

        private void BrowseAndGenerate(params string[] subpaths)
        {
            _filePathFinder.SetPathContext(subpaths);
            var dirs = _filePathFinder.GetDirectoryContents(ListMode.All);
            foreach (var dir in dirs)
            {
                if (dir.IsDirectory)
                {
                    BrowseAndGenerate(subpaths.Union(new[] { dir.Name }).ToArray());
                }
                else if (BookInfoService.DefaultFileContainerExtension.Equals(Path.GetExtension(dir.Name)))
                {
                    var fileInfo = new ThumbnailProvider(_filePathFinder).GetThumbnail(dir.Name);
                    if (!fileInfo.Exists)
                    {
                        _inProgress.Add(new ThumbnailInfo(dir, fileInfo));
                    }
                }
            }
        }

        private void ProcessFile(ThumbnailInfo current)
        {
            try
            {
                var fileContent = new PdfReaderService(current.Book.PhysicalPath).ReadCoverImage();
                var thumbnailContent = _imageService.ScaleAsThumbnail(fileContent);

                var fileInfoPhysicalPath = string.Format("{0}.jpg", current.Book.PhysicalPath);
                using (StreamWriter sw = new StreamWriter(fileInfoPhysicalPath))
                {
                    sw.BaseStream.Write(thumbnailContent, 0, thumbnailContent.Length);
                }
            }
            catch (Exception ex)
            {
                _errors.Add(new ThumbnailWorkerError(current.Book.PhysicalPath, ex));
            }

            current.IsCompleted = true;
        }

        public ThumbnailWorkerStatus GetStatus()
        {
            bool isInProgress = GetTask().Status < TaskStatus.RanToCompletion;
            bool isFaulted = GetTask().IsFaulted || _errors.Any();

            return new ThumbnailWorkerStatus()
                .WithProgress(isInProgress, _inProgress)
                .WithErrors(isFaulted, _errors);
        }
    }
}

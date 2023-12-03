namespace Comicsbox
{
    public class ThumbnailWorkerService : BackgroundService
    {
        private readonly FileMapService _fileMapService;

        private readonly ThumbnailProvider _thumbnailProvider;

        public ThumbnailWorkerService(FileMapService fileMapService, ThumbnailProvider thumbnailProvider)
        {
            _fileMapService = fileMapService;
            _thumbnailProvider = thumbnailProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            Console.WriteLine($"ThumbnailWorker: initializing...");
            while (!stoppingToken.IsCancellationRequested)
            {
                var files = await _fileMapService.GetFileMapAsync();
                Console.WriteLine($"ThumbnailWorker: detected {files.Count()} pdf files.");

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                foreach (var file in files)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var category = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(file))!).ToLower();
                    var isReversed = category == "mangas";
                    await Task.Run(() => _thumbnailProvider.ProcessFile(file, isReversed), stoppingToken);
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                Console.WriteLine($"ThumbnailWorker: waiting 10min...");
                await Task.Delay(10 * 60 * 1000, stoppingToken);
            }

            Console.WriteLine($"ThumbnailWorker: stopped.");
        }
    }
}

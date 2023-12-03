namespace Comicsbox
{
    public class PreCacheWorkerService : BackgroundService
    {
        private readonly FileMapService _fileMapService;

        private readonly BookInfoService _bookInfoService;

        public PreCacheWorkerService(FileMapService fileMapService, BookInfoService bookInfoService)
        {
            _fileMapService = fileMapService;
            _bookInfoService = bookInfoService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var dirs = await _fileMapService.GetDirectoryMapAsync(true);
                Console.WriteLine($"PreCacheWorker: detected {dirs.Count()} directories.");

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                var categories = new List<string>();

                foreach (var dir in dirs)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var category = Path.GetFileName(Path.GetDirectoryName(dir))!;
                    var serie = Path.GetFileName(dir);

                    categories.Add(category);

                    await _bookInfoService.GetBookListAsync(category, serie, true);
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                foreach (var category in categories.Distinct())
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await _bookInfoService.GetBookListAsync(category, "", true);
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                Console.WriteLine($"PreCacheWorker: waiting 10min...");
                await Task.Delay(10 * 60 * 1000, stoppingToken);
            }

            Console.WriteLine($"PreCacheWorker: stopped.");
        }
    }
}

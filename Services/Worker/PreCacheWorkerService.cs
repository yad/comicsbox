namespace Comicsbox
{
    public class PreCacheWorkerService : BackgroundService
    {
        private readonly IConfiguration _configuration;

        private readonly BookInfoService _bookInfoService;

        public PreCacheWorkerService(IConfiguration configuration, BookInfoService bookInfoService)
        {
            _configuration = configuration;
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
                var dirs = await Task.Run(() => Browse(_configuration.GetValue<string>("Settings:AbsoluteBasePath")!));
                Console.WriteLine($"PreCacheWorker: detected {dirs.Count()} directories.");

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                foreach (var dir in dirs)
                {
                    var parts = dir.Split('¤');
                    var category = parts[0];
                    var serie = parts[1];

                    await Task.Run(() => _bookInfoService.GetBookList(category, serie, true), stoppingToken);
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

        private IEnumerable<string> Browse(string path)
        {
            List<string> result = new List<string>();

            foreach (var dir in Directory.GetDirectories(path))
            {
                var category = Path.GetFileName(dir);
                var serie = "";
                result.Add($"{category}¤{serie}");

                foreach (var subdir in Directory.GetDirectories(dir))
                {
                    category = Path.GetFileName(Path.GetDirectoryName(subdir));
                    serie = Path.GetFileName(subdir);
                    result.Add($"{category}¤{serie}");
                }
            }

            return result.Distinct();
        }
    }
}

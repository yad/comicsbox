namespace Comicsbox
{
    public class ThumbnailWorkerService : BackgroundService
    {
        private readonly IConfiguration _configuration;

        private readonly ThumbnailProvider _thumbnailProvider;

        public ThumbnailWorkerService(IConfiguration configuration, ThumbnailProvider thumbnailProvider)
        {
            _configuration = configuration;
            _thumbnailProvider = thumbnailProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var files = await Task.Run(() => Browse(_configuration.GetValue<string>("Settings:AbsoluteBasePath")!));
                Console.WriteLine($"Worker: {files.Count()} loaded.");

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

                    var type = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(file))!).ToLower();
                    var isMangas = type == "mangas";
                    await Task.Run(() => _thumbnailProvider.ProcessFile(file, isMangas), stoppingToken);
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                Console.WriteLine($"Worker: waiting 10min...");
                await Task.Delay(10 * 60 * 1000, stoppingToken);
            }

            Console.WriteLine($"Worker: stopped.");
        }

        private IEnumerable<string> Browse(string path)
        {
            List<string> result = new List<string>();

            foreach (var dir in Directory.GetDirectories(path))
            {
                result.AddRange(Browse(dir));
            }

            foreach (var file in Directory.GetFiles(path))
            {
                result.Add(file);
            }

            return result;
        }
    }
}

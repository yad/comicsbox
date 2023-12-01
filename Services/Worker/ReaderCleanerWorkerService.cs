namespace Comicsbox
{
    public class ReaderCleanerWorkerService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWork(stoppingToken);
        }

        private static IEnumerable<string> GetFinalDirectories(string root, string current)
        {
            List<string> result = new List<string>();

            var dirs = Directory.GetDirectories(current);
            if (!dirs.Any())
            {
                if (current != root)
                {
                    result.Add(current);
                }
            }
            else
            {
                foreach(var dir in dirs)
                {
                    result.AddRange(GetFinalDirectories(root, dir));
                }
            }

            return result;
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            var tempPath = Path.Combine("wwwroot", "temp");
            while (!stoppingToken.IsCancellationRequested)
            {
                var dirs = await Task.Run(() => GetFinalDirectories(tempPath, tempPath));
                Console.WriteLine($"ReaderCleanerWorker: detected {dirs.Count()} directories in temp directory.");

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                foreach (var dir in dirs)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await Task.Run(() =>
                    {
                        try
                        {
                            if (Directory.Exists(dir))
                            {
                                var fileInfo = new FileInfo(dir);
                                if (fileInfo.LastWriteTimeUtc.AddHours(3) < DateTime.UtcNow)
                                {
                                    Directory.Delete(dir, true);
                                    Console.WriteLine($"ReaderCleanerWorker: directory {dir} deleted.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        };
                    }, stoppingToken);
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                Console.WriteLine($"ReaderCleanerWorker: waiting 10min...");
                await Task.Delay(10 * 60 * 1000, stoppingToken);
            }

            Console.WriteLine($"ReaderCleanerWorker: stopped.");
        }
    }
}

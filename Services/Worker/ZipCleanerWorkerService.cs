namespace Comicsbox
{
    public class ZipCleanerWorkerService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            Console.WriteLine($"ZipCleanerWorker: initializing...");
            var tempPath = Path.Combine("wwwroot", "temp");
            while (!stoppingToken.IsCancellationRequested)
            {
                var files = await Task.Run(() => Directory.GetFiles(tempPath, "*.zip"));
                Console.WriteLine($"ZipCleanerWorker: detected {files.Count()} files in temp directory.");

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

                    await Task.Run(() =>
                    {
                        try
                        {
                            if (File.Exists(file))
                            {
                                var fileInfo = new FileInfo(file);
                                if (fileInfo.LastWriteTimeUtc.AddHours(1) < DateTime.UtcNow)
                                {
                                    File.Delete(file);
                                    Console.WriteLine($"ZipCleanerWorker: file {file} deleted.");
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

                Console.WriteLine($"ZipCleanerWorker: waiting 10min...");
                await Task.Delay(10 * 60 * 1000, stoppingToken);
            }

            Console.WriteLine($"ZipCleanerWorker: stopped.");
        }
    }
}

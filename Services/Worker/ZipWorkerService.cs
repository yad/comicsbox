using System.Threading.Channels;

namespace Comicsbox
{
    public class Zip
    {
        public Zip(string name, Func<Task> task)
        {
            Name = name;
            Task = task;
        }

        public string Name { get; private set; }
        public Func<Task> Task { get; private set; }
    }

    public class ZipWorkerService : BackgroundService
    {
        private readonly Channel<Zip> _channel;

        public ZipWorkerService(Channel<Zip> channel)
        {
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"ZipWorker: initializing...");
            while (!_channel.Reader.Completion.IsCompleted && await _channel.Reader.WaitToReadAsync())
            {
                if (_channel.Reader.TryRead(out Zip cmd))
                {
                    Console.WriteLine($"ZipWorker: handling {cmd.Name}...");
                    await cmd.Task.Invoke();
                }
            }
        }
    }
}
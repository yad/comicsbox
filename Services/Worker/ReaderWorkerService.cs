using System.Threading.Channels;

namespace Comicsbox
{
    public class Reader
    {
        public Reader(string name, Func<Task> task)
        {
            Name = name;
            Task = task;
        }

        public string Name { get; private set; }
        public Func<Task> Task { get; private set; }
    }

    public class ReaderWorkerService : BackgroundService
    {
        private readonly Channel<Reader> _channel;

        public ReaderWorkerService(Channel<Reader> channel)
        {
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"ReaderWorker: initializing...");
            while (!_channel.Reader.Completion.IsCompleted && await _channel.Reader.WaitToReadAsync())
            {
                if (_channel.Reader.TryRead(out Reader cmd))
                {
                    Console.WriteLine($"ReaderWorker: handling {cmd.Name}...");
                    await cmd.Task.Invoke();
                }
            }
        }
    }
}
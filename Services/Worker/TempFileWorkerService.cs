using System.Threading.Channels;

namespace Comicsbox
{

    public class TempFileWorkerService : BackgroundService
    {
        private readonly Channel<Func<Task>> _channel;

        public TempFileWorkerService(Channel<Func<Task>> channel)
        {
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!_channel.Reader.Completion.IsCompleted && await _channel.Reader.WaitToReadAsync())
            {
                if (_channel.Reader.TryRead(out Func<Task> cmd))
                {
                    await cmd.Invoke();
                }
            }
        }
    }
}
using System;

namespace Comicsbox.Worker
{
    public class ThumbnailWorkerError
    {
        public string File { get; private set; }

        public Exception Exception { get; private set; }

        public ThumbnailWorkerError(string file, Exception exception)
        {
            File = file;
            Exception = exception;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", File, Exception.Message);
        }
    }
}

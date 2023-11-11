using System.Linq;
using System.Collections.Generic;

namespace Comicsbox.Worker
{
    public class ThumbnailWorkerStatus
    {
        public bool IsInProgress { get; private set; }

        public int InProgressTotalCount { get; private set; }

        public int InProgressCompletedCount { get; private set; }

        public bool IsFaulted { get; private set; }

        public string DisplayErrors { get; private set; }

        public ThumbnailWorkerStatus()
        {
        }

        public ThumbnailWorkerStatus WithProgress(bool isInProgress, IReadOnlyCollection<ThumbnailInfo> inProgress)
        {
            IsInProgress = isInProgress;
            InProgressTotalCount = inProgress.Count();
            InProgressCompletedCount = inProgress.Count(ti => ti.IsCompleted);
            return this;
        }

        public ThumbnailWorkerStatus WithErrors(bool isFaulted, IReadOnlyCollection<ThumbnailWorkerError> errors)
        {
            IsFaulted = isFaulted;
            DisplayErrors = string.Join(" - ", errors.Select(error => error.ToString()));
            return this;
        }
    }
}

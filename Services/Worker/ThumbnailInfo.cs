using Microsoft.Extensions.FileProviders;

namespace Comicsbox.Worker
{
    public class ThumbnailInfo
    {
        public IFileInfo Book { get; private set; }
        public IFileInfo Thumbnail { get; private set; }
        public bool IsCompleted { get; set; }

        public ThumbnailInfo(IFileInfo book, IFileInfo thumbnail)
        {
            Book = book;
            Thumbnail = Thumbnail;
            IsCompleted = false;
        }
    }
}

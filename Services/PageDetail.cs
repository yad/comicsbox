namespace Comicsbox
{
    public class PageDetail
    {
        public static readonly PageDetail NotFound = null;

        public string Content { get; private set; }

        public string ChapterNumber { get; private set; }

        public int PageNumber { get; private set; }

        public PageDetail Previous { get; private set; }

        public PageDetail Next { get; private set; }

        public PageDetail(string chapterNumber, int pageNumber)
        {
            ChapterNumber = chapterNumber;
            PageNumber = pageNumber;
        }

        public PageDetail WithContent (string content)
        {
            Content = content;
            return this;
        }

        public PageDetail WithPrevious(PageDetail previous)
        {
            Previous = previous;
            return this;
        }

        public PageDetail WithNext(PageDetail next)
        {
            Next = next;
            return this;
        }
    }
}
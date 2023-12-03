namespace Comicsbox
{
    public class Book
    {
        public string Name { get; private set; }
        public string Category { get; private set; }

        public string Thumbnail { get; private set; }

        public Book(string name, string category, string thumbnail)
        {
            Name = name;
            Category = category;
            Thumbnail = thumbnail;
        }
    }
}

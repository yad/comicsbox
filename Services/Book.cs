namespace Comicsbox
{
    public class Book
    {
        public string Name { get; private set; }

        public string Thumbnail { get; private set; }

        public Book(string name, string thumbnail)
        {
            Name = name;
            Thumbnail = thumbnail;
        }
    }
}

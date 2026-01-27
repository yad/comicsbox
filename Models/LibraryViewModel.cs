namespace comicsbox.Models;

public class LibraryViewModel
{
    public string? Title { get; set; }          // Titre principal
    public string? Category { get; set; }
    public string? Series { get; set; }

    public List<CardItemViewModel> Items { get; set; } = new();
}
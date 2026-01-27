namespace comicsbox.Models;

public class CardItemViewModel
{
    public string Title { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string Action { get; set; } = "";
    public string Controller { get; set; } = "";
    public object RouteValues { get; set; } = new();
    public bool IsEmoji { get; set; } = false;
}
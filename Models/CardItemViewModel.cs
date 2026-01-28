namespace comicsbox.Models;

public class CardItemViewModel
{
    public string Title { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string Action { get; set; } = "";
    public string Controller { get; set; } = "";
    public object RouteValues { get; set; } = new { }; // on peut garder pour Url.Action
    public bool IsEmoji { get; set; } = false;

    // ✅ Ajouter ces deux propriétés
    public string Category { get; set; } = "";
    public string Series { get; set; } = "";
}

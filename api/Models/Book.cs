namespace api.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    // ISO date string (yyyy-MM-dd) — kept as string to stay simple across the wire.
    public string PublishedDate { get; set; } = string.Empty;
}

namespace api.Models;

public class Quote
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;

    // Featured quotes seeded into the app. Visible to everyone, read-only —
    // nobody can edit or delete them.
    public bool IsSeed { get; set; }

    // Owner of a user-added quote. Null for seeded (read-only) quotes.
    // Only the owner can edit or delete their own quotes.
    public int? UserId { get; set; }
    public User? User { get; set; }
}

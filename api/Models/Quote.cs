namespace api.Models;

public class Quote
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;

    // Owner of the quote. Every quote is visible to all users, but only its
    // owner can edit or delete it.
    public int? UserId { get; set; }
    public User? User { get; set; }
}

namespace api.Models;

public class Quote
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;

    // Owner — "Mina citat" are per-user.
    public int UserId { get; set; }
    public User? User { get; set; }
}

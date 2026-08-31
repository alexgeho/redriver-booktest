namespace api.Dtos;

public record RegisterDto(string Username, string Password);
public record LoginDto(string Username, string Password);
public record AuthResponse(string Token, string Username);

public record BookDto(string Title, string Author, string PublishedDate);
public record QuoteDto(string Text, string Author);

// A quote as returned to clients. `IsSeed` marks the featured read-only quotes
// (shown to everyone, not editable); `Mine` marks the current user's own quotes,
// which are the only ones the UI shows edit/delete controls for.
public record QuoteResponse(int Id, string Text, string Author, bool IsSeed, bool Mine);

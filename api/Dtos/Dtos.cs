namespace api.Dtos;

public record RegisterDto(string Username, string Password);
public record LoginDto(string Username, string Password);
public record AuthResponse(string Token, string Username);

public record BookDto(string Title, string Author, string PublishedDate);
public record QuoteDto(string Text, string Author);

// A quote as returned to clients. Every quote is visible to all users;
// `Mine` tells the frontend whether the current user may edit/delete it,
// and `OwnerUsername` shows who added it.
public record QuoteResponse(int Id, string Text, string Author, string? OwnerUsername, bool Mine);

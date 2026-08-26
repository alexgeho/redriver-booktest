namespace api.Dtos;

public record RegisterDto(string Username, string Password);
public record LoginDto(string Username, string Password);
public record AuthResponse(string Token, string Username);

public record BookDto(string Title, string Author, string PublishedDate);
public record QuoteDto(string Text, string Author);

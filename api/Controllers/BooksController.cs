using api.Data;
using api.Dtos;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _db;
    public BooksController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetAll() =>
        await _db.Books.OrderByDescending(b => b.Id).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Book>> Get(int id)
    {
        var book = await _db.Books.FindAsync(id);
        return book is null ? NotFound() : book;
    }

    [HttpPost]
    public async Task<ActionResult<Book>> Create(BookDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Title is required.");

        var book = new Book
        {
            Title = dto.Title.Trim(),
            Author = dto.Author?.Trim() ?? string.Empty,
            PublishedDate = dto.PublishedDate ?? string.Empty
        };
        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = book.Id }, book);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BookDto dto)
    {
        var book = await _db.Books.FindAsync(id);
        if (book is null) return NotFound();

        book.Title = dto.Title.Trim();
        book.Author = dto.Author?.Trim() ?? string.Empty;
        book.PublishedDate = dto.PublishedDate ?? string.Empty;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book is null) return NotFound();

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

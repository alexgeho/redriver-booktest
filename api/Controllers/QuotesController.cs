using System.Security.Claims;
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
public class QuotesController : ControllerBase
{
    private readonly AppDbContext _db;
    public QuotesController(AppDbContext db) => _db = db;

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // "Mina citat" — only the current user's quotes.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Quote>>> GetMine() =>
        await _db.Quotes
            .Where(q => q.UserId == CurrentUserId)
            .OrderByDescending(q => q.Id)
            .ToListAsync();

    [HttpPost]
    public async Task<ActionResult<Quote>> Create(QuoteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest("Quote text is required.");

        var quote = new Quote
        {
            Text = dto.Text.Trim(),
            Author = dto.Author?.Trim() ?? string.Empty,
            UserId = CurrentUserId
        };
        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMine), new { id = quote.Id }, quote);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, QuoteDto dto)
    {
        var quote = await _db.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == CurrentUserId);
        if (quote is null) return NotFound();

        quote.Text = dto.Text.Trim();
        quote.Author = dto.Author?.Trim() ?? string.Empty;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var quote = await _db.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == CurrentUserId);
        if (quote is null) return NotFound();

        _db.Quotes.Remove(quote);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

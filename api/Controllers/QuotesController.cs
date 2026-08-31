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

    // The 5 featured (read-only) quotes plus the current user's own quotes.
    // Seed quotes come first. `IsSeed` marks read-only ones; `Mine` marks the
    // user's own (the only editable ones).
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuoteResponse>>> GetMine()
    {
        var me = CurrentUserId;
        return await _db.Quotes
            .Where(q => q.IsSeed || q.UserId == me)
            .OrderByDescending(q => q.IsSeed)
            .ThenByDescending(q => q.Id)
            .Select(q => new QuoteResponse(q.Id, q.Text, q.Author, q.IsSeed, !q.IsSeed && q.UserId == me))
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<QuoteResponse>> Create(QuoteDto dto)
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

        var result = new QuoteResponse(quote.Id, quote.Text, quote.Author, false, true);
        return CreatedAtAction(nameof(GetMine), new { id = quote.Id }, result);
    }

    // Update / delete are owner-only and never touch seed quotes: a featured
    // quote or someone else's quote returns 404, so it can be viewed but never
    // changed or removed.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, QuoteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest("Quote text is required.");

        var quote = await _db.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsSeed && q.UserId == CurrentUserId);
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
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsSeed && q.UserId == CurrentUserId);
        if (quote is null) return NotFound();

        _db.Quotes.Remove(quote);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

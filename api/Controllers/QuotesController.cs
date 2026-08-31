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

    // Every quote, from every user, is visible to everyone (newest first).
    // `Mine` marks the ones the current user owns (only those are editable),
    // and `OwnerUsername` shows who added each quote.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuoteResponse>>> GetAll()
    {
        var me = CurrentUserId;
        return await _db.Quotes
            .OrderByDescending(q => q.Id)
            .Select(q => new QuoteResponse(
                q.Id,
                q.Text,
                q.Author,
                q.User != null ? q.User.Username : null,
                q.UserId == me))
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

        var username = User.FindFirstValue(ClaimTypes.Name);
        var result = new QuoteResponse(quote.Id, quote.Text, quote.Author, username, true);
        return CreatedAtAction(nameof(GetAll), new { id = quote.Id }, result);
    }

    // Update / delete are owner-only: a quote you don't own returns 404, so
    // other users can view it but never change or remove it.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, QuoteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest("Quote text is required.");

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

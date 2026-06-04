using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TekkenStats.Application.DTOs.Bookmark;
using TekkenStats.Application.Services;

namespace TekkenStats.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BookmarksController(BookmarkService bookmarkService) : ControllerBase
{
    private int CurrentUserId => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub") ?? "0");

    /// <summary>Get all bookmarked players for the current user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetBookmarks(CancellationToken ct = default)
    {
        var result = await bookmarkService.GetUserBookmarksAsync(CurrentUserId, ct);
        return Ok(result);
    }

    /// <summary>Bookmark a player.</summary>
    [HttpPost]
    public async Task<IActionResult> AddBookmark(AddBookmarkDto dto, CancellationToken ct = default)
    {
        var result = await bookmarkService.AddBookmarkAsync(CurrentUserId, dto.PlayerId, ct);
        if (result is null) return Conflict("Already bookmarked or player not found");
        return CreatedAtAction(nameof(GetBookmarks), result);
    }

    /// <summary>Remove a bookmark.</summary>
    [HttpDelete("{playerId:int}")]
    public async Task<IActionResult> RemoveBookmark(int playerId, CancellationToken ct = default)
    {
        var ok = await bookmarkService.RemoveBookmarkAsync(CurrentUserId, playerId, ct);
        return ok ? NoContent() : NotFound();
    }
}

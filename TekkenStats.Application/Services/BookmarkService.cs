using TekkenStats.Application.DTOs.Bookmark;
using TekkenStats.Application.Interfaces;
using TekkenStats.Domain.Entities;

namespace TekkenStats.Application.Services;

public class BookmarkService(IBookmarkRepository bookmarks, IPlayerRepository players)
{
    public async Task<IEnumerable<BookmarkDto>> GetUserBookmarksAsync(int userId, CancellationToken ct = default)
    {
        var bms = await bookmarks.GetByUserIdAsync(userId, ct);
        return bms.Select(b => new BookmarkDto(
            b.Id, b.PlayerId, b.Player.PlayerName, b.Player.PolarisId,
            b.Player.CurrentRank, b.Player.MainCharacter, b.CreatedAt));
    }

    public async Task<BookmarkDto?> AddBookmarkAsync(int userId, int playerId, CancellationToken ct = default)
    {
        var existing = await bookmarks.GetAsync(userId, playerId, ct);
        if (existing is not null) return null; // already bookmarked

        var player = await players.GetByIdAsync(playerId, ct);
        if (player is null) return null;

        var bookmark = new Bookmark { UserId = userId, PlayerId = playerId };
        await bookmarks.AddAsync(bookmark, ct);
        await bookmarks.SaveChangesAsync(ct);

        return new BookmarkDto(bookmark.Id, playerId, player.PlayerName,
            player.PolarisId, player.CurrentRank, player.MainCharacter, bookmark.CreatedAt);
    }

    public async Task<bool> RemoveBookmarkAsync(int userId, int playerId, CancellationToken ct = default)
    {
        var bookmark = await bookmarks.GetAsync(userId, playerId, ct);
        if (bookmark is null) return false;

        await bookmarks.RemoveAsync(bookmark, ct);
        await bookmarks.SaveChangesAsync(ct);
        return true;
    }
}

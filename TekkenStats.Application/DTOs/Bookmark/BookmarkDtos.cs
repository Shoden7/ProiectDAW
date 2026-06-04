namespace TekkenStats.Application.DTOs.Bookmark;

public record BookmarkDto(
    int Id,
    int PlayerId,
    string PlayerName,
    string PolarisId,
    string CurrentRank,
    string MainCharacter,
    DateTime BookmarkedAt
);

public record AddBookmarkDto(int PlayerId);

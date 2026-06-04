using Microsoft.AspNetCore.Mvc;
using TekkenStats.Application.Services;

namespace TekkenStats.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController(PlayerService playerService) : ControllerBase
{
    /// <summary>Search players by name.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string name,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("name query parameter is required");

        var result = await playerService.SearchPlayersAsync(name, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Get player profile by internal ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var player = await playerService.GetPlayerByIdAsync(id, ct);
        return player is null ? NotFound() : Ok(player);
    }

    /// <summary>Get player profile by Polaris ID.</summary>
    [HttpGet("polaris/{polarisId}")]
    public async Task<IActionResult> GetByPolarisId(string polarisId, CancellationToken ct = default)
    {
        var player = await playerService.GetPlayerByPolarisIdAsync(polarisId, ct);
        return player is null ? NotFound() : Ok(player);
    }

    /// <summary>Get recent matches for a player.</summary>
    [HttpGet("{id:int}/matches")]
    public async Task<IActionResult> GetMatches(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await playerService.GetPlayerMatchesAsync(id, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Global leaderboard ordered by rank then wins.</summary>
    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await playerService.GetLeaderboardAsync(page, pageSize, ct);
        return Ok(result);
    }
}

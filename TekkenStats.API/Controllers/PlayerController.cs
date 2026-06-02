using Microsoft.AspNetCore.Mvc;
using TekkenStats.Domain.Entities;

namespace TekkenStats.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult GetTestPlayer()
        {
            // Now mapping perfectly to your specific Player.cs fields
            var testPlayer = new Player
            {
                Id = 1,
                PolarisId = "POL-7761-9923",
                PlayerName = "KingMain99",
                TekkenUserId = 123456,
                CurrentRank = "Garyu",
                DanRank = 16,
                Wins = 142,
                Losses = 110,
                MainCharacter = "King",
                LastUpdated = DateTime.UtcNow

                // Note: We leave the collection properties (Matches, Bookmarks) 
                // empty for now, which defaults them to empty lists.
            };

            return Ok(new { Message = "TekkenStats API is alive!", Player = testPlayer });
        }
    }
}
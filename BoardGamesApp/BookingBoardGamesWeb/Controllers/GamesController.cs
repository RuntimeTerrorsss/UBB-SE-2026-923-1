using System.Collections.Generic;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly InterfaceGamesRepository _repo;

        public GamesController(InterfaceGamesRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            var game = await _repo.GetGameById(id);
            if (game == null) return NotFound();
            return Ok(game);
        }

        [HttpGet]
        public async Task<ActionResult<List<Game>>> GetAll()
        {
            return Ok(await _repo.GetAll());
        }

        [HttpGet("{id}/price")]
        public async Task<ActionResult<decimal>> GetPrice(int id)
        {
            var game = await _repo.GetGameById(id);
            if (game == null) return NotFound();
            return Ok(game.PricePerDay);
        }

        [HttpPost("search")]
        public async Task<ActionResult<List<Game>>> SearchGames([FromBody] FilterCriteria filter)
        {
            return Ok(await _repo.GetGamesByFilter(filter));
        }

        [HttpGet("feed/tonight")]
        public async Task<ActionResult<List<Game>>> GetGamesFeedAvailableTonight([FromQuery] int userId)
        {
            return Ok(await _repo.GetGamesForFeedAvailableTonight(userId));
        }

        [HttpGet("feed/remaining")]
        public async Task<ActionResult<List<Game>>> GetRemainingGamesForFeed([FromQuery] int userId)
        {
            return Ok(await _repo.GetRemainingGamesForFeed(userId));
        }
    }
}

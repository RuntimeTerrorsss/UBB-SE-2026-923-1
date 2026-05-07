using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGamesWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GamesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null) return NotFound();
            return Ok(game);
        }

        [HttpGet]
        public async Task<ActionResult<List<Game>>> GetAll()
        {
            return await _context.Games.AsNoTracking().ToListAsync();
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<Game>>> Filter([FromQuery] string? name)
        {
            var query = _context.Games.AsQueryable();
            if (!string.IsNullOrEmpty(name)) query = query.Where(g => g.Name.Contains(name));
            return await query.ToListAsync();
        }

        [HttpGet("{id}/price")]
        public async Task<ActionResult<decimal>> GetPrice(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null) return NotFound();
            return Ok(game.PricePerDay);
        }
    }
}

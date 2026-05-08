using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Api.Controllers
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

        [HttpPost("search")]
        public async Task<ActionResult<List<Game>>> SearchGames([FromBody] FilterCriteria filter)
        {
            var query = _context.Games.Include(g => g.Owner).AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(g => g.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrEmpty(filter.City))
            {
                query = query.Where(g => g.Owner.City != null && g.Owner.City.Contains(filter.City));
            }

            if (filter.MaximumPrice.HasValue)
            {
                query = query.Where(g => g.PricePerDay <= filter.MaximumPrice.Value);
            }

            if (filter.PlayerCount.HasValue)
            {
                query = query.Where(g => g.MinimumPlayerNumber <= filter.PlayerCount.Value && 
                                         g.MaximumPlayerNumber >= filter.PlayerCount.Value);
            }

            if (filter.AvailabilityRange != null)
            {
                var startDate = filter.AvailabilityRange.StartTime;
                var endDate = filter.AvailabilityRange.EndTime;
                
                query = query.Where(g => !_context.Rentals
                    .Any(r => r.GameId == g.Id && 
                             ((r.StartDate <= startDate && r.EndDate >= startDate) ||
                              (r.StartDate <= endDate && r.EndDate >= endDate) ||
                              (r.StartDate >= startDate && r.EndDate <= endDate))));
            }

            query = filter.SortOption switch
            {
                SortOption.PriceAscending => query.OrderBy(g => g.PricePerDay),
                SortOption.PriceDescending => query.OrderByDescending(g => g.PricePerDay),
                SortOption.Location => query.OrderBy(g => g.Owner.City),
                _ => query.OrderBy(g => g.Name)
            };

            return await query.ToListAsync();
        }

        [HttpGet("feed/tonight")]
        public async Task<ActionResult<List<Game>>> GetGamesFeedAvailableTonight(int userId)
        {
            var tonight = DateTime.Today;
            var tomorrow = tonight.AddDays(1);

            return await _context.Games
                .Include(g => g.Owner)
                .Where(g => g.IsActive && g.OwnerId != userId)
                .Where(g => !_context.Rentals
                    .Any(r => r.GameId == g.Id && 
                             ((r.StartDate <= tonight && r.EndDate >= tonight) ||
                              (r.StartDate <= tomorrow && r.EndDate >= tomorrow))))
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        [HttpGet("feed/remaining")]
        public async Task<ActionResult<List<Game>>> GetRemainingGamesForFeed(int userId)
        {
            return await _context.Games
                .Include(g => g.Owner)
                .Where(g => g.IsActive && g.OwnerId != userId)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }
    }
}

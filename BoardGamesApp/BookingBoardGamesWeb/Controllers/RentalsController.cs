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
    public class RentalsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RentalsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Rental>> GetRental(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();
            return Ok(rental);
        }

        [HttpGet("game/{gameId}/unavailable")]
        public async Task<ActionResult<List<TimeRange>>> GetUnavailable(int gameId)
        {
            var list = await _context.Rentals
                .Where(r => r.GameId == gameId)
                .Select(r => new TimeRange(r.StartDate, r.EndDate))
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}/timerange")]
        public async Task<ActionResult<TimeRange>> GetRentalTimeRange(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();
            return Ok(new TimeRange(rental.StartDate, rental.EndDate));
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateRental([FromBody] Rental rental)
        {
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();
            return Ok(rental.RentalId);
        }

        [HttpPost("{id}/check")]
        public async Task<ActionResult<bool>> CheckAvailability(int id, [FromBody] TimeRange range)
        {
            bool available = !_context.Rentals.Any(r => r.GameId == id && r.StartDate < range.EndTime && range.StartTime < r.EndDate);
            return Ok(available);
        }
    }
}

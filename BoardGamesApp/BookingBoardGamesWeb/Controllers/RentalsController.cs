using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalRepository _repo;

        public RentalsController(IRentalRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Rental>> GetRental(int id)
        {
            var rental = await _repo.GetById(id);
            if (rental == null) return NotFound();
            return Ok(rental);
        }

        [HttpGet("game/{gameId}/unavailable")]
        public async Task<ActionResult<List<TimeRange>>> GetUnavailable(int gameId)
        {
            return Ok(await _repo.GetUnavailableTimeRanges(gameId));
        }

        [HttpGet("{id}/timerange")]
        public async Task<ActionResult<TimeRange>> GetRentalTimeRange(int id)
        {
            var range = await _repo.GetRentalTimeRange(id);
            if (range == null) return NotFound();
            return Ok(range);
        }

        [HttpPost]
        public async Task<ActionResult> CreateRental([FromBody] Rental rental)
        {
            await _repo.AddRental(rental);
            return Ok(rental.RentalId);
        }

        [HttpPost("{id}/check")]
        public async Task<ActionResult<bool>> CheckAvailability(int id, [FromBody] TimeRange range)
        {
            bool available = await _repo.CheckGameAvailability(range.StartTime, range.EndTime, id);
            return Ok(available);
        }
    }
}

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
        private readonly IRentalRepository rentalRepository;

        public RentalsController(IRentalRepository rentalRepository)
        {
            this.rentalRepository = rentalRepository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Rental>> GetRental(int id)
        {
            var rental = await this.rentalRepository.GetById(id);
            if (rental == null) return NotFound();
            return Ok(rental);
        }

        [HttpGet("game/{gameId}/unavailable")]
        public async Task<ActionResult<List<TimeRange>>> GetUnavailable(int gameId)
        {
            var list = await this.rentalRepository.GetUnavailableTimeRanges(gameId);
            return Ok(list);
        }

        [HttpGet("{id}/timerange")]
        public async Task<ActionResult<TimeRange>> GetRentalTimeRange(int id)
        {
            var range = await this.rentalRepository.GetRentalTimeRange(id);
            if (range == null) return NotFound();
            return Ok(range);
        }

        [HttpPost]
        public async Task<ActionResult> CreateRental([FromBody] Rental rental)
        {
            await this.rentalRepository.AddRental(rental);
            return Ok(rental.RentalId);
        }

        [HttpPost("{id}/check")]
        public async Task<ActionResult<bool>> CheckAvailability(int id, [FromBody] TimeRange range)
        {
            var available = await this.rentalRepository.CheckGameAvailability(range.StartTime, range.EndTime, id);
            return Ok(available);
        }
    }
}

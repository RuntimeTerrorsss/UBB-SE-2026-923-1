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
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        [HttpPut("{id}/address")]
        public async Task<ActionResult> SaveAddress(int id, [FromBody] Address address)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.Country = address.Country;
            user.City = address.City;
            user.Street = address.Street;
            user.StreetNumber = address.StreetNumber;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/balance")]
        public async Task<ActionResult<decimal>> GetBalance(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user.Balance);
        }

        [HttpPut("{id}/balance")]
        public async Task<ActionResult> UpdateBalance(int id, [FromBody] decimal newBalance)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.Balance = newBalance;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

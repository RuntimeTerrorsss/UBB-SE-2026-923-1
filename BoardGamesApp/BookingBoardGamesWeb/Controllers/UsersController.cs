using System.Collections.Generic;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;

        public UsersController(IUserRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _repo.GetById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            return Ok(await _repo.GetAll());
        }

        [HttpPut("{id}/address")]
        public async Task<ActionResult> SaveAddress(int id, [FromBody] Address address)
        {
            await _repo.SaveAddress(id, address);
            return NoContent();
        }

        [HttpGet("{id}/balance")]
        public async Task<ActionResult<decimal>> GetBalance(int id)
        {
            var user = await _repo.GetById(id);
            if (user == null) return NotFound();
            return Ok(user.Balance);
        }

        [HttpPut("{id}/balance")]
        public async Task<ActionResult> UpdateBalance(int id, [FromBody] decimal newBalance)
        {
            await _repo.UpdateBalance(id, newBalance);
            return NoContent();
        }
    }
}

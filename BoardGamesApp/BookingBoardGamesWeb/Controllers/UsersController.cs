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
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            return Ok(await _userRepository.GetAll());
        }

        [HttpPut("{id}/address")]
        public async Task<ActionResult> SaveAddress(int id, [FromBody] Address address)
        {
            await _userRepository.SaveAddress(id, address);
            return NoContent();
        }

        [HttpGet("{id}/balance")]
        public async Task<ActionResult<decimal>> GetBalance(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return NotFound();
            return Ok(user.Balance);
        }

        [HttpPut("{id}/balance")]
        public async Task<ActionResult> UpdateBalance(int id, [FromBody] decimal newBalance)
        {
            await _userRepository.UpdateBalance(id, newBalance);
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.EmailOrUsername) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest();
            }

            var user = await _userRepository.Login(request.EmailOrUsername, request.Password);
            if (user == null) return Unauthorized();

            return Ok(user);
        }

        public class LoginRequest
        {
            public string EmailOrUsername { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] User newUser)
        {
            var success = await _userRepository.Register(newUser);
            if (!success)
            {
                return BadRequest("Registration failed. Username/Email already exists.");
            }
            return Ok();
        }
    }
}

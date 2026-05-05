using Microsoft.AspNetCore.Mvc;
using BookingBoardGames.Data.Repositories;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository repo;

    public UsersController(IUserRepository repo)
    {
        this.repo = repo;
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
        => Ok(repo.GetById(id));

    [HttpPut("{id}/address")]
    public IActionResult SaveAddress(int id, Address address)
    {
        repo.SaveAddress(id, address);
        return Ok();
    }

    [HttpGet("{id}/balance")]
    public IActionResult GetBalance(int id)
        => Ok(repo.GetUserBalance(id));

    [HttpPut("{id}/balance")]
    public IActionResult UpdateBalance(int id, decimal balance)
    {
        repo.UpdateBalance(id, balance);
        return Ok();
    }
}
using BookingBoardGames.Src.Repositories;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/rentals")]
public class RentalsController : ControllerBase
{
    private readonly IRentalRepository repository;

    public RentalsController(IRentalRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var rental = repository.GetById(id);
        return Ok(rental);
    }

    [HttpPost]
    public IActionResult Add(Rental rental)
    {
        repository.AddRental(rental);
        return Ok();
    }
}
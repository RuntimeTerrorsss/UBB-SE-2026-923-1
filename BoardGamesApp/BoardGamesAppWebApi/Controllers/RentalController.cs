using System;
using Microsoft.AspNetCore.Mvc;
using BookingBoardGames.Src.Repositories;


[ApiController]
[Route("api/rentals")]
public class RentalsController : ControllerBase
{
    private readonly IRentalRepository repo;

    public RentalsController(IRentalRepository repo)
    {
        this.repo = repo;
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
        => Ok(repo.GetById(id));

    [HttpPost]
    public IActionResult AddRental(Rental rental)
    {
        repo.AddRental(rental);
        return Ok();
    }

    [HttpGet("unavailable/{gameId}")]
    public IActionResult GetUnavailable(int gameId)
        => Ok(repo.GetUnavailableTimeRanges(gameId));

    [HttpGet("check")]
    public IActionResult Check(DateTime start, DateTime end, int gameId)
        => Ok(repo.CheckGameAvailability(start, end, gameId));
}
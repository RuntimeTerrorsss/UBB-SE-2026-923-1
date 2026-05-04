using Microsoft.AspNetCore.Mvc;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Shared;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly InterfaceGamesRepository repo;

    public GamesController(InterfaceGamesRepository repo)
    {
        this.repo = repo;
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
        => Ok(repo.GetGameById(id));

    [HttpGet("{id}/price")]
    public IActionResult GetPrice(int id)
        => Ok(repo.GetPriceGameById(id));

    [HttpGet("all")]
    public IActionResult GetAll()
        => Ok(repo.GetAll());

    [HttpPost("filter")]
    public IActionResult Filter(FilterCriteria filter)
        => Ok(repo.GetGamesByFilter(filter));

    [HttpGet("feed/available-tonight/{userId}")]
    public IActionResult AvailableTonight(int userId)
        => Ok(repo.GetGamesForFeedAvailableTonight(userId));

    [HttpGet("feed/remaining/{userId}")]
    public IActionResult Remaining(int userId)
        => Ok(repo.GetRemainingGamesForFeed(userId));
}
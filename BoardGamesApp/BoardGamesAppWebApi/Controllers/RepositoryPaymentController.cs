using Microsoft.AspNetCore.Mvc;
using BookingBoardGames.Src.Repositories;


[ApiController]
[Route("api/payments/history")]
public class PaymentsHistoryController : ControllerBase
{
    private readonly IRepositoryPayment repo;

    public PaymentsHistoryController(IRepositoryPayment repo)
    {
        this.repo = repo;
    }

    [HttpGet]
    public IActionResult GetAll()
        => Ok(repo.GetAllPayments());

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
        => Ok(repo.GetPaymentById(id));
}
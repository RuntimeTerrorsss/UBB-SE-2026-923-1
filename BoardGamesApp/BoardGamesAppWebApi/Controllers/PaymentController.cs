using Microsoft.AspNetCore.Mvc;
using BookingBoardGames.Data.Repositories;


[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository repo;

    public PaymentsController(IPaymentRepository repo)
    {
        this.repo = repo;
    }

    [HttpGet]
    public IActionResult GetAll()
        => Ok(repo.GetAllPayments());

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
        => Ok(repo.GetPaymentByIdentifier(id));

    [HttpPost]
    public IActionResult Add(Payment payment)
        => Ok(repo.AddPayment(payment));

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var payment = repo.GetPaymentByIdentifier(id);
        if (payment == null) return NotFound();

        return Ok(repo.DeletePayment(payment));
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Payment payment)
        => Ok(repo.UpdatePayment(payment));
}
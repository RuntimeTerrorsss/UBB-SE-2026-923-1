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
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        [HttpGet]
        public async Task<ActionResult<List<Payment>>> GetAll()
        {
            return await _context.Payments.AsNoTracking().ToListAsync();
        }

        [HttpGet("history")]
        public async Task<ActionResult<List<HistoryPayment>>> GetHistory()
        {
            var query = _context.Payments
                .Include(payment => payment.Request)
                    .ThenInclude(rental => rental.Game)
                .Include(payment => payment.Owner)
                .Select(payment => new HistoryPayment
                {
                    TransactionIdentifier = payment.TransactionIdentifier,
                    PaidAmount = payment.PaidAmount,
                    PaymentMethod = payment.PaymentMethod,
                    DateOfTransaction = payment.DateOfTransaction,
                    DateConfirmedBuyer = payment.DateConfirmedBuyer,
                    DateConfirmedSeller = payment.DateConfirmedSeller,
                    PaymentState = payment.PaymentState,
                    ReceiptFilePath = payment.ReceiptFilePath,
                    RequestId = payment.RequestId,
                    ClientId = payment.ClientId,
                    OwnerId = payment.OwnerId,

                    GameName = payment.Request != null && payment.Request.Game != null
                                    ? payment.Request.Game.Name
                                    : string.Empty,
                    OwnerName = payment.Owner != null
                                    ? payment.Owner.DisplayName
                                    : string.Empty,
                });

            return await query.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<int>> AddPayment([FromBody] Payment payment)
        {
            if (payment.DateOfTransaction == default) payment.DateOfTransaction = System.DateTime.Now;
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return Ok(payment.TransactionIdentifier);
        }
    }
}

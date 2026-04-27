using System.Collections.Generic;
using BookingBoardGames.Repositories;
using BookingBoardGames.Src.Models;

namespace BookingBoardGames.Src.Repositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        IReadOnlyList<Payment> GetAllPayments();

        Payment GetPaymentByIdentifier(int paymentId);

        int AddPayment(Payment payment);

        bool DeletePayment(Payment payment);

        Payment UpdatePayment(Payment payment);
    }
}

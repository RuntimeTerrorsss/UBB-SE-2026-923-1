using System.Collections.Generic;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Model;

namespace BookingBoardGames.Src.Repositories
{
    public interface IRepositoryPayment
    {
        IReadOnlyList<HistoryPayment> GetAllPayments();
        HistoryPayment GetPaymentById(int searchedPaymentId);
    }
}

using System.Collections.Generic;
using BookingBoardGames.Src.Models;
namespace BookingBoardGames.Src.Repositories
{
    public interface IRepositoryPayment
    {
        IReadOnlyList<HistoryPayment> GetAllPayments();
        HistoryPayment GetPaymentById(int searchedPaymentId);
    }
}

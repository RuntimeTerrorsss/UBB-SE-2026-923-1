using System.Collections.Generic;
using BookingBoardGames.Data.Models;

namespace BookingBoardGames.Src.Repositories
{
    /// <summary>
    /// Defines methods for retrieving payment history records from a data source.
    /// </summary>
    public interface IRepositoryPayment
    {
        IReadOnlyList<HistoryPayment> GetAllPayments();

        HistoryPayment GetPaymentById(int searchedPaymentId);
    }
}

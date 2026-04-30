using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Src.Repositories
{
    public class RepositoryPayment : IRepositoryPayment
    {
        private readonly AppDbContext databaseContext;

        public RepositoryPayment(AppDbContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public IReadOnlyList<HistoryPayment> GetAllPayments()
        {
            return this.databaseContext.Payments
                .Where(payment => payment is HistoryPayment)
                .Cast<HistoryPayment>()
                .Include(payment => payment.Client)
                .Include(payment => payment.Owner)
                .ToList();
        }

        public HistoryPayment? GetPaymentById(int searchedPaymentId)
        {
            return this.databaseContext.Payments
                .Where(payment => payment is HistoryPayment)
                .Cast<HistoryPayment>()
                .Include(payment => payment.Client)
                .Include(payment => payment.Owner)
                .FirstOrDefault(payment => payment.TransactionIdentifier == searchedPaymentId);
        }
    }
}

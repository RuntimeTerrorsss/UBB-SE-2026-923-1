using BookingBoardGames.Data;
using BookingBoardGames.Src.Constants;
using BookingBoardGames.Src.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BookingBoardGames.Src.Repositories
{
    public class RepositoryPayment : IRepositoryPayment
    {
        private readonly AppDbContext _context;

        public RepositoryPayment(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<HistoryPayment> BuildPaymentQuery()
        {
            return _context.Payments
                .Include(p => p.Request)
                    .ThenInclude(r => r.Game)
                .Include(p => p.Owner)
                .Select(p => new HistoryPayment
                {
                    TransactionIdentifier = p.TransactionIdentifier,
                    PaidAmount = p.PaidAmount,
                    PaymentMethod = p.PaymentMethod,
                    DateOfTransaction = p.DateOfTransaction,
                    DateConfirmedBuyer = p.DateConfirmedBuyer,
                    DateConfirmedSeller = p.DateConfirmedSeller,
                    PaymentState = p.PaymentState,
                    ReceiptFilePath = p.ReceiptFilePath,
                    RequestId = p.RequestId,
                    ClientId = p.ClientId,
                    OwnerId = p.OwnerId,

                    GameName = p.Request != null && p.Request.Game != null
                                    ? p.Request.Game.Name
                                    : PaymentHistoryConstants.NullGameNameDefaultValue,
                    OwnerName = p.Owner != null
                                    ? p.Owner.DisplayName
                                    : PaymentHistoryConstants.NullOwnerNameDefaultValue,
                });
        }

        public IReadOnlyList<HistoryPayment> GetAllPayments()
        {
            return BuildPaymentQuery().ToList();
        }

        public HistoryPayment? GetPaymentById(int searchedPaymentId)
        {
            return BuildPaymentQuery()
                .FirstOrDefault(p => p.TransactionIdentifier == searchedPaymentId);
        }
    }

}

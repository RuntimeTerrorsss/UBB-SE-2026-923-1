// <copyright file="RepositoryPayment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;
using BookingBoardGames.Src.Constants;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Src.Repositories
{
    public class RepositoryPayment : IRepositoryPayment
    {
        private readonly AppDbContext context;

        public RepositoryPayment(AppDbContext appContext)
        {
            this.context = appContext;
        }

        public IReadOnlyList<HistoryPayment> GetAllPayments()
        {
            return this.BuildPaymentQuery().ToList();
        }

        public HistoryPayment? GetPaymentById(int searchedPaymentId)
        {
            return this.BuildPaymentQuery()
                .FirstOrDefault(p => p.TransactionIdentifier == searchedPaymentId);
        }

        private IQueryable<HistoryPayment> BuildPaymentQuery()
        {
            return this.context.Payments
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
    }
}

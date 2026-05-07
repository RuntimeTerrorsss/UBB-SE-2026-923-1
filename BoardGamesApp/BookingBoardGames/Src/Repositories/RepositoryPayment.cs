// <copyright file="RepositoryPayment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Constants;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Data.Interfaces
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
                .FirstOrDefault(payment => payment.TransactionIdentifier == searchedPaymentId);
        }

        private IQueryable<HistoryPayment> BuildPaymentQuery()
        {
            return this.context.Payments
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
                                    : PaymentHistoryConstants.NullGameNameDefaultValue,
                    OwnerName = payment.Owner != null
                                    ? payment.Owner.DisplayName
                                    : PaymentHistoryConstants.NullOwnerNameDefaultValue,
                });
        }
    }
}

// <copyright file="PaymentRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Src.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Src.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContextFactory contextFactory = new();

        public IReadOnlyList<Payment> GetAllPayments()
        {
            using var context = this.contextFactory.CreateDbContext([]);
            return context.Payments.ToList();
        }

        public virtual Payment GetPaymentByIdentifier(int paymentId)
        {
            using var context = this.contextFactory.CreateDbContext([]);
            return context.Payments.FirstOrDefault(p => p.TransactionIdentifier == paymentId);
        }

        public virtual int AddPayment(Payment payment)
        {
            using var context = this.contextFactory.CreateDbContext([]);
            context.Payments.Add(payment);
            context.SaveChanges();
            return payment.TransactionIdentifier;
        }

        public bool DeletePayment(Payment payment)
        {
            using var context = this.contextFactory.CreateDbContext([]);
            var found = context.Payments.FirstOrDefault(p => p.TransactionIdentifier == payment.TransactionIdentifier);

            if (found is null)
            {
                return false;
            }

            context.Payments.Remove(found);
            context.SaveChanges();
            return true;
        }

        public virtual Payment UpdatePayment(Payment payment)
        {
            using var context = this.contextFactory.CreateDbContext([]);
            var existing = context.Payments.FirstOrDefault(p => p.TransactionIdentifier == payment.TransactionIdentifier);

            if (existing is null)
            {
                return null;
            }

            // Capture previous state to return
            var previousPayment = new Payment
            {
                TransactionIdentifier = existing.TransactionIdentifier,
                RequestId = existing.RequestId,
                ClientId = existing.ClientId,
                OwnerId = existing.OwnerId,
                PaidAmount = existing.PaidAmount,
                PaymentMethod = existing.PaymentMethod,
                DateOfTransaction = existing.DateOfTransaction,
                DateConfirmedBuyer = existing.DateConfirmedBuyer,
                DateConfirmedSeller = existing.DateConfirmedSeller,
                PaymentState = existing.PaymentState,
                ReceiptFilePath = existing.ReceiptFilePath,
            };

            existing.ReceiptFilePath = payment.ReceiptFilePath;
            existing.DateOfTransaction = payment.DateOfTransaction;
            existing.DateConfirmedBuyer = payment.DateConfirmedBuyer;
            existing.DateConfirmedSeller = payment.DateConfirmedSeller;

            context.SaveChanges();

            return previousPayment;
        }

        public Payment? GetGameById(int id)
        {
            throw new System.NotImplementedException();
        }

        public List<Payment> GetAll()
        {
            throw new System.NotImplementedException();
        }
    }
}

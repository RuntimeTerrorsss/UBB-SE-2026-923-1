// <copyright file="PaymentRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;

namespace BookingBoardGames.Src.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext context;

        public PaymentRepository(AppDbContext appContext)
        {
            this.context = appContext;
        }

        public IReadOnlyList<Payment> GetAllPayments()
        {
            return this.context.Payments.ToList();
        }

        public virtual Payment? GetPaymentByIdentifier(int paymentId)
        {
            return this.context.Payments.FirstOrDefault(p => p.TransactionIdentifier == paymentId);
        }

        public virtual int AddPayment(Payment payment)
        {
            if (payment.DateOfTransaction == default)
            {
                payment.DateOfTransaction = DateTime.Now;
            }

            this.context.Payments.Add(payment);
            this.context.SaveChanges();

            return payment.TransactionIdentifier;
        }

        public bool DeletePayment(Payment payment)
        {
            var paymentToDelete = this.context.Payments.Find(payment.TransactionIdentifier);
            if (paymentToDelete == null)
            {
                return false;
            }

            this.context.Payments.Remove(paymentToDelete);
            return this.context.SaveChanges() > 0;
        }

        public virtual Payment? UpdatePayment(Payment payment)
        {
            var existingPayment = this.context.Payments.Find(payment.TransactionIdentifier);

            if (existingPayment == null)
            {
                return null;
            }

            var previousPayment = new Payment
            {
                TransactionIdentifier = existingPayment.TransactionIdentifier,
                RequestId = existingPayment.RequestId,
                ClientId = existingPayment.ClientId,
                OwnerId = existingPayment.OwnerId,
                PaidAmount = existingPayment.PaidAmount,
                ReceiptFilePath = existingPayment.ReceiptFilePath,
                DateOfTransaction = existingPayment.DateOfTransaction,
                DateConfirmedBuyer = existingPayment.DateConfirmedBuyer,
                DateConfirmedSeller = existingPayment.DateConfirmedSeller,
                PaymentMethod = existingPayment.PaymentMethod,
                PaymentState = existingPayment.PaymentState,
            };

            existingPayment.ReceiptFilePath = payment.ReceiptFilePath ?? string.Empty;
            existingPayment.DateOfTransaction = payment.DateOfTransaction ?? DateTime.Now;
            existingPayment.DateConfirmedBuyer = payment.DateConfirmedBuyer;
            existingPayment.DateConfirmedSeller = payment.DateConfirmedSeller;

            this.context.SaveChanges();

            return previousPayment;
        }
    }
}

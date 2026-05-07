// <copyright file="PaymentRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Data.Interfaces
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext context;

        public PaymentRepository(AppDbContext appContext)
        {
            this.context = appContext;
        }

        public async Task<IReadOnlyList<Payment>> GetAllPaymentsAsync()
        {
            return await this.context.Payments.ToListAsync();
        }

        public virtual async Task<Payment?> GetPaymentByIdentifierAsync(int paymentId)
        {
            return await this.context.Payments.FirstOrDefaultAsync(payment => payment.TransactionIdentifier == paymentId);
        }

        public virtual async Task<int> AddPaymentAsync(Payment payment)
        {
            if (payment.DateOfTransaction == default)
            {
                payment.DateOfTransaction = DateTime.Now;
            }

            await this.context.Payments.AddAsync(payment);
            await this.context.SaveChangesAsync();

            return payment.TransactionIdentifier;
        }

        public async Task<bool> DeletePaymentAsync(Payment payment)
        {
            var paymentToDelete = await this.context.Payments.FindAsync(payment.TransactionIdentifier);
            if (paymentToDelete == null)
            {
                return false;
            }

            this.context.Payments.Remove(paymentToDelete);
            return await this.context.SaveChangesAsync() > 0;
        }

        public virtual async Task<Payment?> UpdatePaymentAsync(Payment payment)
        {
            var existingPayment = await this.context.Payments.FindAsync(payment.TransactionIdentifier);

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

            await this.context.SaveChangesAsync();

            return previousPayment;
        }
    }
}

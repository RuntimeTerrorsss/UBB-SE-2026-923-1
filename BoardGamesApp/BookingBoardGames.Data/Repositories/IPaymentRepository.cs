// <copyright file="IPaymentRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace BookingBoardGames.Src.Repositories
{
    /// <summary>
    /// Defines methods for retrieving payment common records from a data source.
    /// </summary>
    public interface IPaymentRepository
    {
        public IReadOnlyList<Payment> GetAllPayments();

        public Payment? GetPaymentByIdentifier(int paymentId);

        public int AddPayment(Payment payment);

        public bool DeletePayment(Payment payment);

        public Payment? UpdatePayment(Payment payment);
    }
}
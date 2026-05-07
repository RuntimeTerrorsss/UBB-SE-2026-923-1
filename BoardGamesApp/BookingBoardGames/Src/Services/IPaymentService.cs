// <copyright file="IPaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BookingBoardGames.Data.Services
{
    public interface IPaymentService
    {
        public void GenerateReceipt(int paymentId);

        public string GetReceipt(int paymentId);
    }
}

// <copyright file="IPaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace BookingBoardGames.Src.Services
{
    public interface IPaymentService
    {
        public void GenerateReceipt(int paymentId);

        public Task<string> GetReceipt(int paymentId);
    }
}

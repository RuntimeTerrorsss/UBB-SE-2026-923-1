// <copyright file="IReceiptService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BookingBoardGames.Src.Services
{
    public interface IReceiptService
    {
        public string GenerateReceiptRelativePath(int rentalId);

        public string GetReceiptDocument(Payment payment);
    }
}

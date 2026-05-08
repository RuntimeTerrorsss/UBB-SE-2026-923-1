// <copyright file="IReceiptService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BookingBoardgamesILoveBan.Src.Receipt.Service
{
    public interface IReceiptService
    {
        void GenerateReceipt(int paymentId);
    }
}

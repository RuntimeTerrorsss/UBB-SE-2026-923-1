// <copyright file="IPaymentRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BookingBoardgamesILoveBan.Src.PaymentCommon.Repository
{
    public interface IPaymentRepository
    {
        BookingBoardgamesILoveBan.Src.PaymentCommon.Model.Payment? GetPaymentById(int id);
    }
}

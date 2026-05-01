// <copyright file="ICardPaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BookingBoardGames.Data;
using BookingBoardGames.Src.DTO;

namespace BookingBoardGames.Src.Services
{
    public interface ICardPaymentService
    {
        CardPaymentDTO AddCardPayment(int requestIdentifier, int clientIdentifier, int ownerIdentifier, decimal amount);

        bool CheckBalanceSufficiency(int requestIdentifier, int clientIdentifier);

        CardPaymentDTO GetCardPayment(int paymentIdentifier);

        decimal GetCurrentBalance(int clientIdentifier);

        void ProcessPayment(int rentalIdentifier, int clientIdentifier, int ownerIdentifier);

        CardPaymentDTO ConvertToDataTransferObject(Payment cardPayment);

        RentalDataTransferObject GetRequestDataTransferObject(int rentalIdentifier);
    }
}

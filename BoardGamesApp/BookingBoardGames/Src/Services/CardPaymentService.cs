// <copyright file="CardPaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using BookingBoardGames.Src.Constants;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Repositories;

namespace BookingBoardGames.Src.Services
{
    public class CardPaymentService : PaymentService, ICardPaymentService
    {
        private readonly IUserRepository userRepository;
        private readonly IRentalService rentalService;

        public CardPaymentService(
            PaymentRepositoryHttp paymentRepository,
            IUserRepository userRepository,
            ReceiptService receiptService,
            IRentalService rentalService)
            : base(paymentRepository, receiptService)
        {
            this.userRepository = userRepository;
            this.rentalService = rentalService;
        }

        public virtual CardPaymentDTO AddCardPayment(int requestIdentifier, int clientIdentifier, int ownerIdentifier, decimal amount)
        {
            if (!this.CheckBalanceSufficiency(requestIdentifier, clientIdentifier))
            {
                throw new Exception("Insufficient Funds");
            }

            this.ProcessPayment(requestIdentifier, clientIdentifier, ownerIdentifier);

            Payment payment = new Payment
            {
                RequestId = requestIdentifier,
                ClientId = clientIdentifier,
                OwnerId = ownerIdentifier,
                PaidAmount = amount,
                PaymentMethod = CardPaymentConstants.CardPaymentMethodName,
                DateOfTransaction = DateTime.Now,
                DateConfirmedBuyer = DateTime.Now,
                DateConfirmedSeller = null,
                PaymentState = CardPaymentConstants.SuccessfulPaymentState,
                ReceiptFilePath = null,
            };

            payment.TransactionIdentifier = this.paymentRepository.AddPayment(payment);
            string receiptFilePath = this.receiptService.GenerateReceiptRelativePath(payment.RequestId);
            payment.ReceiptFilePath = receiptFilePath;
            this.paymentRepository.UpdatePayment(payment);

            return this.ConvertToDataTransferObject(payment);
        }

        public bool CheckBalanceSufficiency(int requestIdentifier, int clientIdentifier)
        {
            return this.rentalService.GetRentalPrice(requestIdentifier) <= this.userRepository.GetUserBalance(clientIdentifier);
        }

        public CardPaymentDTO GetCardPayment(int paymentIdentifier)
        {
            return this.ConvertToDataTransferObject(this.paymentRepository.GetPaymentByIdentifier(paymentIdentifier));
        }

        public decimal GetCurrentBalance(int clientIdentifier)
        {
            return this.userRepository.GetUserBalance(clientIdentifier);
        }

        public void ProcessPayment(int rentalIdentifier, int clientIdentifier, int ownerIdentifier)
        {
            decimal rentalPrice = this.rentalService.GetRentalPrice(rentalIdentifier);
            decimal clientBalance = this.userRepository.GetUserBalance(clientIdentifier);
            decimal ownerBalance = this.userRepository.GetUserBalance(ownerIdentifier);
            decimal newClientBalance = clientBalance - rentalPrice;

            if (newClientBalance < 0)
            {
                throw new Exception("Insufficient Funds");
            }

            this.userRepository.UpdateBalance(clientIdentifier, newClientBalance);
            this.userRepository.UpdateBalance(ownerIdentifier, ownerBalance + rentalPrice);
        }

        public CardPaymentDTO ConvertToDataTransferObject(Payment cardPayment)
        {
            return new CardPaymentDTO(
                    transactionIdentifier: cardPayment.TransactionIdentifier,
                    requestIdentifier: cardPayment.RequestId,
                    clientIdentifier: cardPayment.ClientId,
                    ownerIdentifier: cardPayment.OwnerId,
                    amount: cardPayment.PaidAmount,
                    dateOfTransaction: cardPayment.DateOfTransaction ?? DateTime.Now,
                    paymentMethod: cardPayment.PaymentMethod);
        }

        public virtual RentalDataTransferObject GetRequestDataTransferObject(int rentalIdentifier)
        {
            Rental rental = this.rentalService.GetRentalById(rentalIdentifier);
            string gameName = this.rentalService.GetGameName(rental.RentalId);
            string ownerName = this.userRepository.GetById(rental.OwnerId).Username;
            string clientName = this.userRepository.GetById(rental.ClientId).Username;
            decimal gamePrice = this.rentalService.GetRentalPrice(rental.RentalId);

            return new RentalDataTransferObject(rental.RentalId, rental.GameId, gameName, rental.ClientId, clientName, rental.OwnerId, ownerName, rental.StartDate, rental.EndDate, gamePrice);
        }
    }
}

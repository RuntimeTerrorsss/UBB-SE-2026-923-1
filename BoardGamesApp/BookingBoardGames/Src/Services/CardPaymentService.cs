// <copyright file="CardPaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BookingBoardGames.Data.Constants;
using BookingBoardGames.Data.DTO;
using BookingBoardGames.Data.Interfaces;

namespace BookingBoardGames.Data.Services
{
    public class CardPaymentService : PaymentService, ICardPaymentService
    {
        private readonly IUserRepository userRepository;
        private readonly IRentalService rentalService;

        public CardPaymentService(
            PaymentRepository paymentRepository,
            IUserRepository userRepository,
            ReceiptService receiptService,
            IRentalService rentalService)
            : base(paymentRepository, receiptService)
        {
            this.userRepository = userRepository;
            this.rentalService = rentalService;
        }

        public virtual async Task<CardPaymentDTO> AddCardPayment(int requestIdentifier, int clientIdentifier, int ownerIdentifier, decimal amount)
        {
            if (!(await this.CheckBalanceSufficiency(requestIdentifier, clientIdentifier)))
            {
                throw new Exception("Insufficient Funds");
            }

            await this.ProcessPayment(requestIdentifier, clientIdentifier, ownerIdentifier);

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

            payment.TransactionIdentifier = await this.paymentRepository.AddPaymentAsync(payment);
            string receiptFilePath = this.receiptService.GenerateReceiptRelativePath(payment.RequestId);
            payment.ReceiptFilePath = receiptFilePath;
            await this.paymentRepository.UpdatePaymentAsync(payment);

            return this.ConvertToDataTransferObject(payment);
        }

        public async Task<bool> CheckBalanceSufficiency(int requestIdentifier, int clientIdentifier)
        {
            return await this.rentalService.GetRentalPrice(requestIdentifier) <= this.userRepository.GetUserBalance(clientIdentifier);
        }

        public async Task<CardPaymentDTO?> GetCardPaymentAsync(int paymentIdentifier)
        {
            var payment = await this.paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);
            return payment == null ? null : this.ConvertToDataTransferObject(payment);
        }

        public decimal GetCurrentBalance(int clientIdentifier)
        {
            return this.userRepository.GetUserBalance(clientIdentifier);
        }

        public async Task ProcessPayment(int rentalIdentifier, int clientIdentifier, int ownerIdentifier)
        {
            decimal rentalPrice = await this.rentalService.GetRentalPrice(rentalIdentifier);
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

        public virtual async Task<RentalDataTransferObject> GetRequestDataTransferObject(int rentalIdentifier)
        {
            Rental rental = await this.rentalService.GetRentalById(rentalIdentifier);
            string gameName = await this.rentalService.GetGameName(rental.RentalId);
            string ownerName = this.userRepository.GetById(rental.OwnerId).Username;
            string clientName = this.userRepository.GetById(rental.ClientId).Username;
            decimal gamePrice = await this.rentalService.GetRentalPrice(rental.RentalId);

            return new RentalDataTransferObject(rental.RentalId, rental.GameId, gameName, rental.ClientId, clientName, rental.OwnerId, ownerName, rental.StartDate, rental.EndDate, gamePrice);
        }
    }
}

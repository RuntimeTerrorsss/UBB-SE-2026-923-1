using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Constants;
using BookingBoardGames.Src.DTO;
using System;

namespace BookingBoardGames.Src.Services
{
    public class CardPaymentService : PaymentService
    {
        private readonly IUserRepository userRepository;
        private readonly IRentalService rentalService;

        public CardPaymentService(
            PaymentRepository paymentRepository,
            IUserRepository userRepository,
            ReceiptService receiptService,
            IRentalService requestService) : base(paymentRepository, receiptService)
        {
            this.userRepository = userRepository;
            this.rentalService = requestService;
        }

        public virtual CardPaymentDataTransferObject AddCardPayment(int requestIdentifier, int clientIdentifier, int ownerIdentifier, decimal amount)
        {
            if (!CheckBalanceSufficiency(requestIdentifier, clientIdentifier))
            {
                throw new Exception("Insufficient Funds");
            }

            ProcessPayment(requestIdentifier, clientIdentifier, ownerIdentifier);

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
                ReceiptFilePath = null
            };

            payment.TransactionIdentifier = paymentRepository.AddPayment(payment);
            string receiptFilePath = receiptService.GenerateReceiptRelativePath(payment.RequestId);
            payment.ReceiptFilePath = receiptFilePath;
            paymentRepository.UpdatePayment(payment);

            return ConvertToDataTransferObject(payment);
        }

        public bool CheckBalanceSufficiency(int requestIdentifier, int clientIdentifier)
        {
            return rentalService.GetRentalPrice(requestIdentifier) <= userRepository.GetUserBalance(clientIdentifier);
        }

        public CardPaymentDataTransferObject GetCardPayment(int paymentIdentifier)
        {
            return this.ConvertToDataTransferObject(paymentRepository.GetPaymentByIdentifier(paymentIdentifier));
        }

        public decimal GetCurrentBalance(int clientIdentifier)
        {
            return userRepository.GetUserBalance(clientIdentifier);
        }

        public void ProcessPayment(int rentalIdentifier, int clientIdentifier, int ownerIdentifier)
        {
            decimal rentalPrice = rentalService.GetRentalPrice(rentalIdentifier);
            decimal clientBalance = userRepository.GetUserBalance(clientIdentifier);
            decimal ownerBalance = userRepository.GetUserBalance(ownerIdentifier);
            decimal newClientBalance = clientBalance - rentalPrice;

            if (newClientBalance < 0)
            {
                throw new Exception("Insufficient Funds");
            }

            userRepository.UpdateBalance(clientIdentifier, newClientBalance);
            userRepository.UpdateBalance(ownerIdentifier, ownerBalance + rentalPrice);
        }

        public CardPaymentDataTransferObject ConvertToDataTransferObject(Payment cardPayment)
        {
            return new CardPaymentDataTransferObject(
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
            Rental rental = rentalService.GetRentalById(rentalIdentifier);
            string gameName = rentalService.GetGameName(rental.RentalId);
            string ownerName = userRepository.GetById(rental.OwnerId).Username;
            string clientName = userRepository.GetById(rental.ClientId).Username;
            decimal gamePrice = rentalService.GetRentalPrice(rental.RentalId);

            return new RentalDataTransferObject(rental.RentalId, rental.GameId, gameName, rental.ClientId, clientName, rental.OwnerId, ownerName, rental.StartDate, rental.EndDate, gamePrice);
        }
    }
}

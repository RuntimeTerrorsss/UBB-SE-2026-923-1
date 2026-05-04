using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames;
using BookingBoardGames;
using Microsoft.EntityFrameworkCore;
using BookingBoardGames.Data;
using System;
using Xunit;
using BookingBoardGames.Src.Services;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;

namespace BookingBoardGames.Tests.PaymentCard
{
    public class CardPaymentIntegrationTests
    {
        public CardPaymentIntegrationTests()
        {
            DatabaseBootstrap.Initialize();
        }

        [Fact]
        public void AddCardPayment_ValidPipeline_ReturnsNotNullResult()
        {
            PaymentRepository paymentRepository = new PaymentRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            UserRepository userService = new UserRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            GamesRepository GamesRepository = new GamesRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            RentalRepository RentalRepository = new RentalRepository(null);
            RentalService RentalService = new RentalService(RentalRepository, GamesRepository);
            ReceiptService receiptService = new ReceiptService(userService, RentalService, GamesRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, RentalService);

            int clientIdentifier = 5;
            int ownerIdentifier = 2;
            int RequestIdentifier = 5;
            decimal paymentPrice = 15m;

            decimal currentClientBalance = userService.GetUserBalance(clientIdentifier);
            decimal currentOwnerBalance = userService.GetUserBalance(ownerIdentifier);

            try
            {
                var resultDataTransferObject = cardPaymentService.AddCardPayment(RequestIdentifier, clientIdentifier, ownerIdentifier, paymentPrice);
                Assert.NotNull(resultDataTransferObject);
            }
            finally
            {
                userService.UpdateBalance(clientIdentifier, currentClientBalance);
                userService.UpdateBalance(ownerIdentifier, currentOwnerBalance);
            }
        }

        [Fact]
        public void AddCardPayment_ValidPipeline_ReturnsCardPaymentMethod()
        {
            PaymentRepository paymentRepository = new PaymentRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            UserRepository userService = new UserRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            GamesRepository GamesRepository = new GamesRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            RentalRepository RentalRepository = new RentalRepository(null);
            RentalService RentalService = new RentalService(RentalRepository, GamesRepository);
            ReceiptService receiptService = new ReceiptService(userService, RentalService, GamesRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, RentalService);

            int clientIdentifier = 5;
            int ownerIdentifier = 2;
            int RequestIdentifier = 5;
            decimal paymentPrice = 15m;
            string expectedPaymentMethod = "CARD";

            decimal currentClientBalance = userService.GetUserBalance(clientIdentifier);
            decimal currentOwnerBalance = userService.GetUserBalance(ownerIdentifier);

            try
            {
                var resultDataTransferObject = cardPaymentService.AddCardPayment(RequestIdentifier, clientIdentifier, ownerIdentifier, paymentPrice);
                Assert.Equal(expectedPaymentMethod, resultDataTransferObject.PaymentMethod);
            }
            finally
            {
                userService.UpdateBalance(clientIdentifier, currentClientBalance);
                userService.UpdateBalance(ownerIdentifier, currentOwnerBalance);
            }
        }

        [Fact]
        public void GetCardPayment_ValidTransaction_ReturnsNotNull()
        {
            PaymentRepository paymentRepository = new PaymentRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            UserRepository userService = new UserRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            GamesRepository GamesRepository = new GamesRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            RentalRepository RentalRepository = new RentalRepository(null);
            RentalService RentalService = new RentalService(RentalRepository, GamesRepository);
            ReceiptService receiptService = new ReceiptService(userService, RentalService, GamesRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, RentalService);

            int clientIdentifier = 5;
            int ownerIdentifier = 2;
            int RequestIdentifier = 5;
            decimal paymentPrice = 15m;

            decimal currentClientBalance = userService.GetUserBalance(clientIdentifier);
            decimal currentOwnerBalance = userService.GetUserBalance(ownerIdentifier);

            try
            {
                var resultDataTransferObject = cardPaymentService.AddCardPayment(RequestIdentifier, clientIdentifier, ownerIdentifier, paymentPrice);
                var retrievedPayment = cardPaymentService.GetCardPayment(resultDataTransferObject.TransactionIdentifier);
                Assert.NotNull(retrievedPayment);
            }
            finally
            {
                userService.UpdateBalance(clientIdentifier, currentClientBalance);
                userService.UpdateBalance(ownerIdentifier, currentOwnerBalance);
            }
        }

        [Fact]
        public void GetCardPayment_ValidTransaction_ReturnsCorrectAmount()
        {
            PaymentRepository paymentRepository = new PaymentRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            UserRepository userService = new UserRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            GamesRepository GamesRepository = new GamesRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            RentalRepository RentalRepository = new RentalRepository(null);
            RentalService RentalService = new RentalService(RentalRepository, GamesRepository);
            ReceiptService receiptService = new ReceiptService(userService, RentalService, GamesRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, RentalService);

            int clientIdentifier = 5;
            int ownerIdentifier = 2;
            int RequestIdentifier = 5;
            decimal paymentPrice = 15m;

            decimal currentClientBalance = userService.GetUserBalance(clientIdentifier);
            decimal currentOwnerBalance = userService.GetUserBalance(ownerIdentifier);

            try
            {
                var resultDataTransferObject = cardPaymentService.AddCardPayment(RequestIdentifier, clientIdentifier, ownerIdentifier, paymentPrice);
                var retrievedPayment = cardPaymentService.GetCardPayment(resultDataTransferObject.TransactionIdentifier);
                Assert.Equal(paymentPrice, retrievedPayment.Amount);
            }
            finally
            {
                userService.UpdateBalance(clientIdentifier, currentClientBalance);
                userService.UpdateBalance(ownerIdentifier, currentOwnerBalance);
            }
        }

        [Fact]
        public void GetCardPayment_ValidTransaction_ReturnsCorrectClientIdentifier()
        {
            PaymentRepository paymentRepository = new PaymentRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            UserRepository userService = new UserRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            GamesRepository GamesRepository = new GamesRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            RentalRepository RentalRepository = new RentalRepository(null);
            RentalService RentalService = new RentalService(RentalRepository, GamesRepository);
            ReceiptService receiptService = new ReceiptService(userService, RentalService, GamesRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, RentalService);

            int clientIdentifier = 5;
            int ownerIdentifier = 2;
            int RequestIdentifier = 5;
            decimal paymentPrice = 15m;

            decimal currentClientBalance = userService.GetUserBalance(clientIdentifier);
            decimal currentOwnerBalance = userService.GetUserBalance(ownerIdentifier);

            try
            {
                var resultDataTransferObject = cardPaymentService.AddCardPayment(RequestIdentifier, clientIdentifier, ownerIdentifier, paymentPrice);
                var retrievedPayment = cardPaymentService.GetCardPayment(resultDataTransferObject.TransactionIdentifier);
                Assert.Equal(clientIdentifier, retrievedPayment.ClientIdentifier);
            }
            finally
            {
                userService.UpdateBalance(clientIdentifier, currentClientBalance);
                userService.UpdateBalance(ownerIdentifier, currentOwnerBalance);
            }
        }

        [Fact]
        public void AddCardPayment_InsufficientFunds_ThrowsException()
        {
            PaymentRepository paymentRepository = new PaymentRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            UserRepository userService = new UserRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            GamesRepository GamesRepository = new GamesRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            RentalRepository RentalRepository = new RentalRepository(null);
            RentalService RentalService = new RentalService(RentalRepository, GamesRepository);
            ReceiptService receiptService = new ReceiptService(userService, RentalService, GamesRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, RentalService);

            int lowBalanceClientIdentifier = 8;
            int ownerIdentifier = 2;
            int RequestIdentifier = 5;
            decimal paymentPrice = 15m;

            Assert.Throws<Exception>(() => cardPaymentService.AddCardPayment(RequestIdentifier, lowBalanceClientIdentifier, ownerIdentifier, paymentPrice));
        }
    }
}






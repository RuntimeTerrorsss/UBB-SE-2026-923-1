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
            RentalRepository RentalRepository = new RentalRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
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
            RentalRepository RentalRepository = new RentalRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
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
            RentalRepository RentalRepository = new RentalRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
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
            RentalRepository RentalRepository = new RentalRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
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
            RentalRepository RentalRepository = new RentalRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
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
            var dbContext = new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>());
            PaymentRepository paymentRepository = new PaymentRepository(dbContext);
            UserRepository userService = new UserRepository(dbContext);
            GamesRepository GamesRepository = new GamesRepository(dbContext);
            RentalRepository RentalRepository = new RentalRepository(dbContext);
            RentalService RentalService = new RentalService(RentalRepository, GamesRepository);
            ReceiptService receiptService = new ReceiptService(userService, RentalService, GamesRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, RentalService);

            // Find users and rentals by property to be ID-independent
            var frank = dbContext.Users.First(u => u.Username == "frank_06");
            var alice = dbContext.Users.First(u => u.Username == "alice99");
            var rentalForFrank = dbContext.Rentals.First(r => r.ClientId == frank.Id);

            int lowBalanceClientIdentifier = frank.Id;
            int ownerIdentifier = alice.Id;
            int requestIdentifier = rentalForFrank.RentalId;
            decimal paymentPrice = 15m;

            // Frank has 10.00 balance, Rental 5 (Seven Wonders) has price 48.00.
            Assert.Throws<Exception>(() => cardPaymentService.AddCardPayment(requestIdentifier, lowBalanceClientIdentifier, ownerIdentifier, paymentPrice));
        }
    }
}






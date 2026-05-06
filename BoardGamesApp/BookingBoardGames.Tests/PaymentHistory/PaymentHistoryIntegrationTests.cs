using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames;
using BookingBoardGames;
using Microsoft.EntityFrameworkCore;
using BookingBoardGames.Data;
using BookingBoardGames.Src.Enum;
using BookingBoardGames.Src.Shared;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;
using BookingBoardGames.Src.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardGames.Tests.PaymentHistory
{
    public class PaymentHistoryIntegrationTests
    {
        private IServicePayment servicePayment;

        public PaymentHistoryIntegrationTests()
        {
            DatabaseBootstrap.Initialize();
        }

        [Fact]
        public void CalculateTotalAmount_NonEmptyDatabase_ReturnsValidDataAndPositiveTotal()
        {
            RepositoryPayment repositoryPayment = new RepositoryPayment(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            UserRepository userRepository = new UserRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            GamesAPIProxy GamesRepository = new GamesAPIProxy(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            RentalRepository RentalRepository = new RentalRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            RentalService RentalService = new RentalService(RentalRepository, GamesRepository);
            ReceiptService receiptService = new ReceiptService(userRepository, RentalService, GamesRepository);

            servicePayment = new ServicePayment(repositoryPayment, receiptService);

            var allPayments = servicePayment.GetAllPaymentsForUI();
            var totalAmount = servicePayment.CalculateTotalAmount(allPayments);

            Assert.NotEmpty(allPayments);
            Assert.True(totalAmount > 0);
        }

        [Fact]
        public void GetReceiptDocumentPath_ForFilteredPayments_ReturnsValidPathAndCorrectResults()
        {
            RepositoryPayment repositoryPayment = new RepositoryPayment(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            UserRepository userRepository = new UserRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            GamesAPIProxy GamesRepository = new GamesAPIProxy(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            RentalRepository RentalRepository = new RentalRepository(new AppDbContextFactory().CreateDbContext(System.Array.Empty<string>()));
            RentalService RentalService = new RentalService(RentalRepository, GamesRepository);
            ReceiptService receiptService = new ReceiptService(userRepository, RentalService, GamesRepository);

            servicePayment = new ServicePayment(repositoryPayment, receiptService);

            var receiptPath = servicePayment.GetReceiptDocumentPath(5);
            var filteredPaymentsByCard = servicePayment.GetFilteredPayments(FilterType.AllTime, PaymentMethod.CARD, pageNumber: 1, pageSize: 5);

            Assert.EndsWith(".pdf", receiptPath);
            Assert.All(filteredPaymentsByCard.Items, payment => Assert.Equal("CARD", payment.PaymentMethod, ignoreCase: true));
        }
    }
}







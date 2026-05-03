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
            RepositoryPayment repositoryPayment = new RepositoryPayment();
            UserRepository userRepository = new UserRepository();
            GameRepository gameRepository = new GameRepository();
            RequestRepository requestRepository = new RequestRepository();
            RequestService requestService = new RequestService(requestRepository, gameRepository);
            ReceiptService receiptService = new ReceiptService(userRepository, requestService, gameRepository);

            servicePayment = new ServicePayment(repositoryPayment, receiptService);

            var allPayments = servicePayment.GetAllPaymentsForUI();
            var totalAmount = servicePayment.CalculateTotalAmount(allPayments);

            Assert.NotEmpty(allPayments);
            Assert.True(totalAmount > 0);
        }

        [Fact]
        public void GetReceiptDocumentPath_ForFilteredPayments_ReturnsValidPathAndCorrectResults()
        {
            RepositoryPayment repositoryPayment = new RepositoryPayment();
            UserRepository userRepository = new UserRepository();
            GameRepository gameRepository = new GameRepository();
            RequestRepository requestRepository = new RequestRepository();
            RequestService requestService = new RequestService(requestRepository, gameRepository);
            ReceiptService receiptService = new ReceiptService(userRepository, requestService, gameRepository);

            servicePayment = new ServicePayment(repositoryPayment, receiptService);

            var receiptPath = servicePayment.GetReceiptDocumentPath(5);
            var filteredPaymentsByCard = servicePayment.GetFilteredPayments(FilterType.AllTime, PaymentMethod.CARD, pageNumber: 1, pageSize: 5);

            Assert.EndsWith(".pdf", receiptPath);
            Assert.All(filteredPaymentsByCard.Items, payment => Assert.Equal("CARD", payment.PaymentMethod, ignoreCase: true));
        }
    }
}

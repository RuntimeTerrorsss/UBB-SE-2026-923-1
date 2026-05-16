using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingBoardGames.Data.Constants;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Services;
using Moq;
using Xunit;


namespace BookingBoardGames.Tests.Services
{
    public class ServicePaymentTests : IDisposable
    {
        private readonly Mock<IRepositoryPayment> _mockPaymentRepository;
        private readonly Mock<IReceiptService> _mockReceiptService;
        private readonly ServicePayment _service;

        public ServicePaymentTests()
        {
            _mockPaymentRepository = new Mock<IRepositoryPayment>();
            _mockReceiptService = new Mock<IReceiptService>();
            _service = new ServicePayment(_mockPaymentRepository.Object, _mockReceiptService.Object);

            // Default Mock Setup for Session Context
            // Assumes your SessionContext singleton has a settable UserId for testing contexts.
            SessionContext.GetInstance().UserId = 1;
        }

        public void Dispose()
        {
            // Reset SessionContext after each test to prevent cross-test pollution
            SessionContext.GetInstance().UserId = 0;
        }

        #region GetAllPaymentsForUI & CurrentUser Filtering

        [Fact]
        public async Task GetAllPaymentsForUI_SessionUserIdZero_ReturnsEmptyList()
        {
            // Arrange
            SessionContext.GetInstance().UserId = 0;
            _mockPaymentRepository.Setup(r => r.GetAllPayments()).ReturnsAsync(GetDummyHistoryPayments());

            // Act
            var result = await _service.GetAllPaymentsForUI();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPaymentsForUI_ValidSessionUser_FiltersByClientOrOwnerAndMapsCorrectly()
        {
            // Arrange
            SessionContext.GetInstance().UserId = 1; // User 1 is Client for P1, Owner for P2
            _mockPaymentRepository.Setup(r => r.GetAllPayments()).ReturnsAsync(GetDummyHistoryPayments());

            // Act
            var result = await _service.GetAllPaymentsForUI();

            // Assert
            Assert.Equal(2, result.Count);

            // Verifies fallback mapping logic for null properties
            var paymentWithNulls = result.First(p => p.PaymentId == 2);
            Assert.Equal(PaymentHistoryConstants.NullGameNameDefaultValue, paymentWithNulls.ProductName);
            Assert.Equal(PaymentHistoryConstants.NullOwnerNameDefaultValue, paymentWithNulls.ReceiverName);
            Assert.Equal(PaymentHistoryConstants.NullDateOfTransactionDefaultValue, paymentWithNulls.DateText);
        }

        #endregion

        #region GetFilteredPayments - Date Filters

        [Theory]
        [InlineData(FilterType.Last3Months, 1)]  // Only P1 is recent (within 3 months)
        [InlineData(FilterType.Last6Months, 2)]  // P1 and P3 (4 months ago)
        [InlineData(FilterType.Last9Months, 3)]  // P1, P3, and P4 (8 months ago)
        [InlineData(FilterType.AllTime, 4)]      // All 4 valid for User 1
        public async Task GetFilteredPayments_DateFilters_AppliesDateThresholdsCorrectly(FilterType filter, int expectedCount)
        {
            // Arrange
            SessionContext.GetInstance().UserId = 1;

            var payments = new List<HistoryPayment>
            {
                new HistoryPayment { TransactionIdentifier = 1, ClientId = 1, DateOfTransaction = DateTime.Now.AddMonths(-1) },
                new HistoryPayment { TransactionIdentifier = 2, ClientId = 1, DateOfTransaction = DateTime.Now.AddMonths(-4) },
                new HistoryPayment { TransactionIdentifier = 3, ClientId = 1, DateOfTransaction = DateTime.Now.AddMonths(-8) },
                new HistoryPayment { TransactionIdentifier = 4, ClientId = 1, DateOfTransaction = DateTime.Now.AddMonths(-12) }
            };

            _mockPaymentRepository.Setup(r => r.GetAllPayments()).ReturnsAsync(payments);

            // Act
            var result = await _service.GetFilteredPayments(filter);

            // Assert
            Assert.Equal(expectedCount, result.TotalCount);
            Assert.Equal(expectedCount, result.Items.Count());
        }

        #endregion

        #region GetFilteredPayments - Search and Payment Method

        [Fact]
        public async Task GetFilteredPayments_PaymentMethodApplied_FiltersCorrectly()
        {
            // Arrange
            SessionContext.GetInstance().UserId = 1;
            _mockPaymentRepository.Setup(r => r.GetAllPayments()).ReturnsAsync(GetDummyHistoryPayments());

            // Act
            var result = await _service.GetFilteredPayments(FilterType.AllTime, PaymentMethod.CASH);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("cash", result.Items.First().PaymentMethod);
        }

        [Fact]
        public async Task GetFilteredPayments_SearchQueryApplied_FiltersByGameNameCaseInsensitive()
        {
            // Arrange
            SessionContext.GetInstance().UserId = 1;
            _mockPaymentRepository.Setup(r => r.GetAllPayments()).ReturnsAsync(GetDummyHistoryPayments());

            // Act - Searching for 'cata' should match 'Catan'
            var result = await _service.GetFilteredPayments(FilterType.AllTime, PaymentMethod.ALL, "cata");

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("Catan", result.Items.First().ProductName);
        }

        #endregion

        #region GetFilteredPayments - Sorting

        [Theory]
        [InlineData(FilterType.AlphabeticalAsc, "Catan")]
        [InlineData(FilterType.AlphabeticalDesc, PaymentHistoryConstants.NullGameNameDefaultValue)] // "z" fallback puts it first descending
        [InlineData(FilterType.Newest, "Catan")] // P1 is the newest (today)
        [InlineData(FilterType.Oldest, PaymentHistoryConstants.NullGameNameDefaultValue)] // P2 has null date, defaults to MinValue
        public async Task GetFilteredPayments_Sorting_OrdersItemsCorrectly(FilterType filter, string expectedFirstProductName)
        {
            // Arrange
            SessionContext.GetInstance().UserId = 1;
            _mockPaymentRepository.Setup(r => r.GetAllPayments()).ReturnsAsync(GetDummyHistoryPayments());

            // Act
            var result = await _service.GetFilteredPayments(filter);

            // Assert
            Assert.Equal(expectedFirstProductName, result.Items.First().ProductName);
        }

        #endregion

        #region GetFilteredPayments - Pagination

        [Fact]
        public async Task GetFilteredPayments_Pagination_SkipsAndTakesCorrectly()
        {
            // Arrange
            SessionContext.GetInstance().UserId = 1;
            var payments = Enumerable.Range(1, 15).Select(i => new HistoryPayment { TransactionIdentifier = i, ClientId = 1 }).ToList();
            _mockPaymentRepository.Setup(r => r.GetAllPayments()).ReturnsAsync(payments);

            // Act - Page 2, Size 5 (Should return items 6 through 10)
            var result = await _service.GetFilteredPayments(FilterType.AllTime, pageNumber: 2, pageSize: 5);

            // Assert
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(5, result.Items.Count());
            Assert.Equal(6, result.Items.First().PaymentId);
            Assert.Equal(10, result.Items.Last().PaymentId);
        }

        #endregion

        #region CalculateTotalAmount

        [Fact]
        public void CalculateTotalAmount_NullList_ReturnsDefaultValue()
        {
            // Act
            var result = _service.CalculateTotalAmount(null);

            // Assert
            Assert.Equal(PaymentHistoryConstants.NullAmountDefaultValue, result);
        }

        [Fact]
        public void CalculateTotalAmount_ValidList_ReturnsSumOfAmounts()
        {
            // Arrange
            var items = new List<PaymentDataTransferObject>
            {
                new PaymentDataTransferObject { Amount = 10.5m },
                new PaymentDataTransferObject { Amount = 20.0m }
            };

            // Act
            var result = _service.CalculateTotalAmount(items);

            // Assert
            Assert.Equal(30.5m, result);
        }

        #endregion

        #region GetReceiptDocumentPath

        [Fact]
        public async Task GetReceiptDocumentPath_NullPath_GeneratesPathAndFetchesDocument()
        {
            // Arrange
            // FIX: Instantiating as HistoryPayment instead of Payment
            var payment = new HistoryPayment { RequestId = 5, ReceiptFilePath = null };
            string generatedPath = "receipts\\generated_123.pdf";
            string fullPath = "C:\\Documents\\receipts\\generated_123.pdf";

            _mockPaymentRepository.Setup(r => r.GetPaymentById(1)).ReturnsAsync(payment);
            _mockReceiptService.Setup(s => s.GenerateReceiptRelativePath(5)).Returns(generatedPath);
            _mockReceiptService.Setup(s => s.GetReceiptDocument(payment)).ReturnsAsync(fullPath);

            // Act
            var result = await _service.GetReceiptDocumentPath(1);

            // Assert
            Assert.Equal(generatedPath, payment.ReceiptFilePath);
            Assert.Equal(fullPath, result);
            _mockReceiptService.Verify(s => s.GetReceiptDocument(payment), Times.Once);
        }

        [Fact]
        public async Task GetReceiptDocumentPath_PathWithoutSlash_PrependsReceiptsFolder()
        {
            // Arrange
            // FIX: Instantiating as HistoryPayment instead of Payment
            var payment = new HistoryPayment { RequestId = 5, ReceiptFilePath = "no_slash_file.pdf" };

            _mockPaymentRepository.Setup(r => r.GetPaymentById(1)).ReturnsAsync(payment);
            _mockReceiptService.Setup(s => s.GetReceiptDocument(payment)).ReturnsAsync("C:\\full\\path.pdf");

            // Act
            await _service.GetReceiptDocumentPath(1);

            // Assert
            Assert.Equal("receipts\\no_slash_file.pdf", payment.ReceiptFilePath);
            _mockReceiptService.Verify(s => s.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetReceiptDocumentPath_ValidPath_FetchesDocumentDirectly()
        {
            // Arrange
            // FIX: Instantiating as HistoryPayment instead of Payment
            var payment = new HistoryPayment { RequestId = 5, ReceiptFilePath = "receipts\\valid_file.pdf" };

            _mockPaymentRepository.Setup(r => r.GetPaymentById(1)).ReturnsAsync(payment);
            _mockReceiptService.Setup(s => s.GetReceiptDocument(payment)).ReturnsAsync("C:\\full\\path.pdf");

            // Act
            await _service.GetReceiptDocumentPath(1);

            // Assert
            Assert.Equal("receipts\\valid_file.pdf", payment.ReceiptFilePath);
            _mockReceiptService.Verify(s => s.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region Helper Data Providers

        private List<HistoryPayment> GetDummyHistoryPayments()
        {
            return new List<HistoryPayment>
            {
                // P1: Client is User 1 (Should be included), Has fully valid data
                new HistoryPayment
                {
                    TransactionIdentifier = 1, ClientId = 1, OwnerId = 99,
                    DateOfTransaction = DateTime.Now, GameName = "Catan", OwnerName = "Alice",
                    PaidAmount = 15m, PaymentMethod = "card", ReceiptFilePath = "receipts\\1.pdf"
                },
                // P2: Owner is User 1 (Should be included), Has nulls to hit fallback branches
                new HistoryPayment
                {
                    TransactionIdentifier = 2, ClientId = 99, OwnerId = 1,
                    DateOfTransaction = null, GameName = null, OwnerName = null,
                    PaidAmount = 25m, PaymentMethod = "cash", ReceiptFilePath = null
                },
                // P3: Unrelated User (Should be filtered out)
                new HistoryPayment
                {
                    TransactionIdentifier = 3, ClientId = 99, OwnerId = 100,
                    DateOfTransaction = DateTime.Now, GameName = "Monopoly"
                }
            };
        }

        #endregion
    }
}
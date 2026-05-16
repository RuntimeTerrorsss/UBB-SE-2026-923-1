using System;
using System.Threading.Tasks;
using BookingBoardGames.Data.Constants;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Services;
using Moq;
using Xunit;

namespace BookingBoardGames.Tests.Services
{
    public class CardPaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IReceiptService> _mockReceiptService;
        private readonly Mock<IRentalService> _mockRentalService;
        private readonly CardPaymentService _cardPaymentService;

        public CardPaymentServiceTests()
        {
            _mockPaymentRepository = new Mock<IPaymentRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockReceiptService = new Mock<IReceiptService>();
            _mockRentalService = new Mock<IRentalService>();

            _cardPaymentService = new CardPaymentService(
                _mockPaymentRepository.Object,
                _mockUserRepository.Object,
                _mockReceiptService.Object,
                _mockRentalService.Object);
        }

        #region AddCardPayment

        [Fact]
        public async Task AddCardPayment_InsufficientBalance_ThrowsException()
        {
            // Arrange
            int requestId = 1, clientId = 2, ownerId = 3;
            decimal amount = 50m;

            _mockRentalService.Setup(r => r.GetRentalPrice(requestId)).ReturnsAsync(100m);
            _mockUserRepository.Setup(u => u.GetUserBalance(clientId)).ReturnsAsync(50m); // 50 < 100

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _cardPaymentService.AddCardPayment(requestId, clientId, ownerId, amount));

            Assert.Equal("Insufficient Funds", exception.Message);
        }

        [Fact]
        public async Task AddCardPayment_ValidData_ProcessesPaymentAndReturnsDTO()
        {
            // Arrange
            int requestId = 1, clientId = 2, ownerId = 3;
            decimal amount = 50m;
            decimal rentalPrice = 50m;
            decimal clientBalance = 100m;
            decimal ownerBalance = 200m;
            int newTransactionId = 99;
            string receiptPath = "/receipts/1.pdf";

            _mockRentalService.Setup(r => r.GetRentalPrice(requestId)).ReturnsAsync(rentalPrice);
            _mockUserRepository.Setup(u => u.GetUserBalance(clientId)).ReturnsAsync(clientBalance);
            _mockUserRepository.Setup(u => u.GetUserBalance(ownerId)).ReturnsAsync(ownerBalance);

            _mockPaymentRepository.Setup(p => p.AddPaymentAsync(It.IsAny<Payment>())).ReturnsAsync(newTransactionId);
            _mockReceiptService.Setup(r => r.GenerateReceiptRelativePath(requestId)).Returns(receiptPath);

            // Act
            var result = await _cardPaymentService.AddCardPayment(requestId, clientId, ownerId, amount);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newTransactionId, result.TransactionIdentifier);
            Assert.Equal(requestId, result.RequestIdentifier);

            _mockUserRepository.Verify(u => u.UpdateBalance(clientId, clientBalance - rentalPrice), Times.Once);
            _mockUserRepository.Verify(u => u.UpdateBalance(ownerId, ownerBalance + rentalPrice), Times.Once);
            _mockPaymentRepository.Verify(p => p.UpdatePaymentAsync(It.Is<Payment>(pay => pay.ReceiptFilePath == receiptPath)), Times.Once);
        }

        #endregion

        #region CheckBalanceSufficiency

        [Theory]
        [InlineData(50, 100, true)]  // Price < Balance
        [InlineData(100, 100, true)] // Price == Balance
        [InlineData(150, 100, false)] // Price > Balance
        public async Task CheckBalanceSufficiency_VariousBalances_ReturnsExpectedResult(decimal price, decimal balance, bool expectedResult)
        {
            // Arrange
            int requestId = 1, clientId = 2;
            _mockRentalService.Setup(r => r.GetRentalPrice(requestId)).ReturnsAsync(price);
            _mockUserRepository.Setup(u => u.GetUserBalance(clientId)).ReturnsAsync(balance);

            // Act
            var result = await _cardPaymentService.CheckBalanceSufficiency(requestId, clientId);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        #endregion

        #region GetCardPaymentAsync

        [Fact]
        public async Task GetCardPaymentAsync_PaymentNotFound_ReturnsNull()
        {
            // Arrange
            int paymentId = 1;
            _mockPaymentRepository.Setup(p => p.GetPaymentByIdentifierAsync(paymentId)).ReturnsAsync((Payment)null);

            // Act
            var result = await _cardPaymentService.GetCardPaymentAsync(paymentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCardPaymentAsync_PaymentFound_ReturnsDTO()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment
            {
                TransactionIdentifier = paymentId,
                RequestId = 2,
                ClientId = 3,
                OwnerId = 4,
                PaidAmount = 100m,
                PaymentMethod = CardPaymentConstants.CardPaymentMethodName,
                DateOfTransaction = DateTime.Now
            };

            _mockPaymentRepository.Setup(p => p.GetPaymentByIdentifierAsync(paymentId)).ReturnsAsync(payment);

            // Act
            var result = await _cardPaymentService.GetCardPaymentAsync(paymentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(paymentId, result.TransactionIdentifier);
            Assert.Equal(payment.PaidAmount, result.Amount);
        }

        #endregion

        #region GetCurrentBalance

        [Fact]
        public async Task GetCurrentBalance_ValidClient_ReturnsBalance()
        {
            // Arrange
            int clientId = 1;
            decimal expectedBalance = 250.5m;
            _mockUserRepository.Setup(u => u.GetUserBalance(clientId)).ReturnsAsync(expectedBalance);

            // Act
            var result = await _cardPaymentService.GetCurrentBalance(clientId);

            // Assert
            Assert.Equal(expectedBalance, result);
        }

        #endregion

        #region ProcessPayment

        [Fact]
        public async Task ProcessPayment_InsufficientFunds_ThrowsException()
        {
            // Arrange
            int rentalId = 1, clientId = 2, ownerId = 3;
            _mockRentalService.Setup(r => r.GetRentalPrice(rentalId)).ReturnsAsync(100m);
            _mockUserRepository.Setup(u => u.GetUserBalance(clientId)).ReturnsAsync(50m); // Balance < Price

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _cardPaymentService.ProcessPayment(rentalId, clientId, ownerId));

            Assert.Equal("Insufficient Funds", exception.Message);
        }

        [Fact]
        public async Task ProcessPayment_SufficientFunds_UpdatesBalances()
        {
            // Arrange
            int rentalId = 1, clientId = 2, ownerId = 3;
            decimal rentalPrice = 100m;
            decimal clientBalance = 150m;
            decimal ownerBalance = 200m;

            _mockRentalService.Setup(r => r.GetRentalPrice(rentalId)).ReturnsAsync(rentalPrice);
            _mockUserRepository.Setup(u => u.GetUserBalance(clientId)).ReturnsAsync(clientBalance);
            _mockUserRepository.Setup(u => u.GetUserBalance(ownerId)).ReturnsAsync(ownerBalance);

            // Act
            await _cardPaymentService.ProcessPayment(rentalId, clientId, ownerId);

            // Assert
            _mockUserRepository.Verify(u => u.UpdateBalance(clientId, 50m), Times.Once); // 150 - 100
            _mockUserRepository.Verify(u => u.UpdateBalance(ownerId, 300m), Times.Once); // 200 + 100
        }

        #endregion

        #region ConvertToDataTransferObject

        [Fact]
        public void ConvertToDataTransferObject_DateNull_UsesCurrentDate()
        {
            // Arrange
            var payment = new Payment
            {
                TransactionIdentifier = 1,
                DateOfTransaction = null
            };
            var beforeExecution = DateTime.Now;

            // Act
            var result = _cardPaymentService.ConvertToDataTransferObject(payment);

            // Assert
            Assert.True(result.DateOfTransaction >= beforeExecution);
            Assert.True(result.DateOfTransaction <= DateTime.Now);
        }

        [Fact]
        public void ConvertToDataTransferObject_DateNotNull_UsesProvidedDate()
        {
            // Arrange
            var specificDate = new DateTime(2025, 1, 1);
            var payment = new Payment
            {
                TransactionIdentifier = 1,
                DateOfTransaction = specificDate
            };

            // Act
            var result = _cardPaymentService.ConvertToDataTransferObject(payment);

            // Assert
            Assert.Equal(specificDate, result.DateOfTransaction);
        }

        #endregion

        #region GetRequestDataTransferObject

        [Fact]
        public async Task GetRequestDataTransferObject_RentalIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            int rentalId = 1;
            _mockRentalService.Setup(r => r.GetRentalById(rentalId)).ReturnsAsync((Rental)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _cardPaymentService.GetRequestDataTransferObject(rentalId));

            Assert.Equal($"Rental with ID {rentalId} was not found.", exception.Message);
        }

        [Fact]
        public async Task GetRequestDataTransferObject_UsersAreNull_UsesFallbackNames()
        {
            // Arrange
            int rentalId = 1;
            var rental = new Rental { RentalId = rentalId, GameId = 2, OwnerId = 3, ClientId = 4, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };

            _mockRentalService.Setup(r => r.GetRentalById(rentalId)).ReturnsAsync(rental);
            _mockRentalService.Setup(r => r.GetGameName(rentalId)).ReturnsAsync("Catan");
            _mockRentalService.Setup(r => r.GetRentalPrice(rentalId)).ReturnsAsync(50m);

            _mockUserRepository.Setup(u => u.GetById(rental.OwnerId)).ReturnsAsync((User)null);
            _mockUserRepository.Setup(u => u.GetById(rental.ClientId)).ReturnsAsync((User)null);

            // Act
            var result = await _cardPaymentService.GetRequestDataTransferObject(rentalId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Unknown Owner", result.OwnerName);
            Assert.Equal("Unknown Client", result.ClientName);
            Assert.Equal("Catan", result.GameName);
        }

        [Fact]
        public async Task GetRequestDataTransferObject_ValidData_ReturnsFullyPopulatedDTO()
        {
            // Arrange
            int rentalId = 1;
            var rental = new Rental { RentalId = rentalId, GameId = 2, OwnerId = 3, ClientId = 4, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };
            var owner = new User { Id = 3, Username = "Alice" };
            var client = new User { Id = 4, Username = "Bob" };

            _mockRentalService.Setup(r => r.GetRentalById(rentalId)).ReturnsAsync(rental);
            _mockRentalService.Setup(r => r.GetGameName(rentalId)).ReturnsAsync("Monopoly");
            _mockRentalService.Setup(r => r.GetRentalPrice(rentalId)).ReturnsAsync(30m);

            _mockUserRepository.Setup(u => u.GetById(rental.OwnerId)).ReturnsAsync(owner);
            _mockUserRepository.Setup(u => u.GetById(rental.ClientId)).ReturnsAsync(client);

            // Act
            var result = await _cardPaymentService.GetRequestDataTransferObject(rentalId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(rentalId, result.Id);
            Assert.Equal("Alice", result.OwnerName);
            Assert.Equal("Bob", result.ClientName);
            Assert.Equal("Monopoly", result.GameName);
            Assert.Equal(30m, result.Price);
        }

        #endregion
    }
}
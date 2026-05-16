using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.Services;
using Moq;
using Xunit;

// Note: Replace 'YourDomainNamespace' with the actual namespace where your Payment, User, Rental, and Game entities are located.

namespace BookingBoardGames.Tests.Services
{
    public class ReceiptServiceTests : IDisposable
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRentalService> _mockRentalService;
        private readonly Mock<InterfaceGamesRepository> _mockGameRepository;
        private readonly ReceiptService _receiptService;

        // Used to track and clean up PDF files generated in the real MyDocuments folder during testing
        private readonly List<string> _filesToCleanup = new();

        public ReceiptServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRentalService = new Mock<IRentalService>();
            _mockGameRepository = new Mock<InterfaceGamesRepository>();

            _receiptService = new ReceiptService(
                _mockUserRepository.Object,
                _mockRentalService.Object,
                _mockGameRepository.Object);

            SetupDefaultMocks();
        }

        public void Dispose()
        {
            // Clean up any files created on the disk during the tests
            foreach (var filePath in _filesToCleanup)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    {
                        // Ignore exceptions during cleanup in tests
                    }
                }
            }
        }

        private void SetupDefaultMocks()
        {
            _mockRentalService.Setup(r => r.GetRentalById(It.IsAny<int>()))
                .ReturnsAsync(new Rental { GameId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(3) });

            _mockGameRepository.Setup(g => g.GetGameById(It.IsAny<int>()))
                .ReturnsAsync(new Game { Name = "Test Boardgame" });

            _mockUserRepository.Setup(u => u.GetById(It.IsAny<int>()))
                .ReturnsAsync(new User { Username = "TestUser" });
        }

        #region GenerateReceiptRelativePath

        [Fact]
        public void GenerateReceiptRelativePath_ValidId_ReturnsExpectedFormat()
        {
            // Arrange
            int requestId = 99;

            // Act
            string result = _receiptService.GenerateReceiptRelativePath(requestId);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("receipts\\receipt_99_", result);
            Assert.EndsWith(".pdf", result);
        }

        #endregion

        #region GetReceiptDocument - Edge Cases

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetReceiptDocument_ReceiptPathIsNullOrEmpty_ThrowsInvalidOperationException(string invalidPath)
        {
            // Arrange
            var payment = new Payment { ReceiptFilePath = invalidPath };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _receiptService.GetReceiptDocument(payment));

            Assert.Equal("Receipt path is missing.", exception.Message);
        }

        [Fact]
        public async Task GetReceiptDocument_ReceiptPathIsWhiteSpace_ThrowsFromPrepareDocumentPath()
        {
            // Arrange
            // A string with only spaces bypasses the (path == string.Empty) check in GetReceiptDocument,
            // but correctly triggers the string.IsNullOrWhiteSpace() check inside PrepareDocumentPath.
            var payment = new Payment { ReceiptFilePath = "   " };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _receiptService.GetReceiptDocument(payment));

            Assert.Equal("Receipt path is missing.", exception.Message);
        }

        #endregion

        #region GetReceiptDocument - File Exists Logic

        [Fact]
        public async Task GetReceiptDocument_FileAlreadyExists_ReturnsPathWithoutRecreating()
        {
            // Arrange
            var payment = new Payment
            {
                RequestId = 1,
                PaymentMethod = "card",
                ReceiptFilePath = "receipts\\test_existing_receipt.pdf"
            };

            // Act 1: Call once to create the file physically on disk
            string createdPath = await _receiptService.GetReceiptDocument(payment);
            _filesToCleanup.Add(createdPath);

            // Act 2: Call again. Because it exists, it should NOT rebuild the document.
            string existingPath = await _receiptService.GetReceiptDocument(payment);

            // Assert
            Assert.Equal(createdPath, existingPath);
            Assert.True(File.Exists(existingPath));

            // Verify the mocks were only called ONCE (during the first creation), proving the 2nd call bypassed generation
            _mockRentalService.Verify(r => r.GetRentalById(It.IsAny<int>()), Times.Once);
        }

        #endregion

        #region GetReceiptDocument - Creation and Content Generation Branches

        [Fact]
        public async Task GetReceiptDocument_FileDoesNotExist_CashPayment_CreatesPdfAndHitsCatchBlockForDate()
        {
            // Arrange
            var payment = new Payment
            {
                RequestId = 2,
                ClientId = 10,
                OwnerId = 20,
                PaidAmount = 50m,
                PaymentMethod = "cash", // Hits the cash branch in BuildConfirmation
                DateConfirmedSeller = DateTime.Now,
                DateConfirmedBuyer = DateTime.Now,
                // Using a malformed filename to guarantee the DateTime.ParseExact fails 
                // and hits the try-catch fallback branch in GetIssuedDateFromFilename
                ReceiptFilePath = "receipts\\bad_format_name.pdf"
            };

            // Act
            string generatedPath = await _receiptService.GetReceiptDocument(payment);
            _filesToCleanup.Add(generatedPath);

            // Assert
            Assert.True(File.Exists(generatedPath));

            _mockRentalService.Verify(r => r.GetRentalById(payment.RequestId), Times.Once);
            _mockGameRepository.Verify(g => g.GetGameById(It.IsAny<int>()), Times.Once);
            _mockUserRepository.Verify(u => u.GetById(payment.ClientId), Times.Once);
            _mockUserRepository.Verify(u => u.GetById(payment.OwnerId), Times.Once);
        }

        [Fact]
        public async Task GetReceiptDocument_FileDoesNotExist_CardPayment_CreatesPdfWithValidGeneratedPath()
        {
            // Arrange
            int requestId = 3;
            // Generate a valid path using the service itself to give ParseExact the best chance of succeeding
            string validRelativePath = _receiptService.GenerateReceiptRelativePath(requestId);

            var payment = new Payment
            {
                RequestId = requestId,
                ClientId = 10,
                OwnerId = 20,
                PaidAmount = 100m,
                PaymentMethod = "card", // Hits the non-cash branch in BuildConfirmation
                DateOfTransaction = DateTime.Now,
                ReceiptFilePath = validRelativePath
            };

            // Act
            string generatedPath = await _receiptService.GetReceiptDocument(payment);
            _filesToCleanup.Add(generatedPath);

            // Assert
            Assert.True(File.Exists(generatedPath));

            _mockRentalService.Verify(r => r.GetRentalById(payment.RequestId), Times.Once);
        }

        #endregion
    }
}
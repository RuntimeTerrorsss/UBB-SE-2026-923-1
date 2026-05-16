using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.Services;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace BookingBoardGames.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
        private readonly Mock<IReceiptService> _mockReceiptService;
        private readonly TestablePaymentService _paymentService;

        public PaymentServiceTests()
        {
            _mockPaymentRepository = new Mock<IPaymentRepository>();
            _mockReceiptService = new Mock<IReceiptService>();

            _paymentService = new TestablePaymentService(
                _mockPaymentRepository.Object,
                _mockReceiptService.Object);
        }

        #region GenerateReceiptAsync

        [Fact]
        public async Task GenerateReceiptAsync_PaymentNotFound_DoesNothing()
        {
            // Arrange
            int paymentId = 1;
            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync((Payment)null);

            // Act
            await _paymentService.GenerateReceiptAsync(paymentId);

            // Assert
            _mockReceiptService.Verify(s => s.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
            _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(It.IsAny<Payment>()), Times.Never);
        }

        [Fact]
        public async Task GenerateReceiptAsync_PaymentFound_UpdatesPaymentWithReceiptPath()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment { RequestId = 100, ReceiptFilePath = null };
            string generatedPath = "/receipts/100.pdf";

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            _mockReceiptService.Setup(s => s.GenerateReceiptRelativePath(payment.RequestId))
                               .Returns(generatedPath);

            // Act
            await _paymentService.GenerateReceiptAsync(paymentId);

            // Assert
            Assert.Equal(generatedPath, payment.ReceiptFilePath);
            _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(payment), Times.Once);
        }

        #endregion

        #region GetReceiptAsync

        [Fact]
        public async Task GetReceiptAsync_PaymentNotFound_ReturnsEmptyString()
        {
            // Arrange
            int paymentId = 1;
            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync((Payment)null);

            // Act
            var result = await _paymentService.GetReceiptAsync(paymentId);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetReceiptAsync_ReceiptPathAlreadyExists_ReturnsDocumentDirectly()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment { RequestId = 100, ReceiptFilePath = "/receipts/100.pdf" };
            string expectedDocument = "Base64PDFContentOrFullPath";

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            _mockReceiptService.Setup(s => s.GetReceiptDocument(payment))
                               .ReturnsAsync(expectedDocument);

            // Act
            var result = await _paymentService.GetReceiptAsync(paymentId);

            // Assert
            Assert.Equal(expectedDocument, result);

            // Verify GenerateReceiptAsync logic was skipped
            _mockReceiptService.Verify(s => s.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
            _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(It.IsAny<Payment>()), Times.Never);
        }

        [Fact]
        public async Task GetReceiptAsync_ReceiptPathEmpty_GeneratesPathAndReturnsDocument()
        {
            // Arrange
            int paymentId = 1;
            var paymentWithoutPath = new Payment { RequestId = 100, ReceiptFilePath = null };
            var paymentWithPath = new Payment { RequestId = 100, ReceiptFilePath = "/receipts/100.pdf" };

            string generatedPath = "/receipts/100.pdf";
            string expectedDocument = "Base64PDFContentOrFullPath";

            // In GetReceiptAsync, if path is null, it calls GetPaymentByIdentifierAsync 3 times:
            // 1. Initial check in GetReceiptAsync
            // 2. Inside GenerateReceiptAsync
            // 3. Second check in GetReceiptAsync after generation
            _mockPaymentRepository.SetupSequence(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(paymentWithoutPath) // 1st call
                                  .ReturnsAsync(paymentWithoutPath) // 2nd call (inside GenerateReceiptAsync)
                                  .ReturnsAsync(paymentWithPath);   // 3rd call

            _mockReceiptService.Setup(s => s.GenerateReceiptRelativePath(paymentWithoutPath.RequestId))
                               .Returns(generatedPath);

            _mockReceiptService.Setup(s => s.GetReceiptDocument(paymentWithPath))
                               .ReturnsAsync(expectedDocument);

            // Act
            var result = await _paymentService.GetReceiptAsync(paymentId);

            // Assert
            Assert.Equal(expectedDocument, result);
            _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(paymentWithoutPath), Times.Once);
        }

        [Fact]
        public async Task GetReceiptAsync_PaymentDisappearsAfterGeneration_ReturnsEmptyString()
        {
            // Arrange
            int paymentId = 1;
            var paymentWithoutPath = new Payment { RequestId = 100, ReceiptFilePath = null };
            string generatedPath = "/receipts/100.pdf";

            // Setup sequence to simulate the payment being deleted/unavailable after generation
            _mockPaymentRepository.SetupSequence(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(paymentWithoutPath) // 1st call
                                  .ReturnsAsync(paymentWithoutPath) // 2nd call (inside GenerateReceiptAsync)
                                  .ReturnsAsync((Payment)null);     // 3rd call (After generation)

            _mockReceiptService.Setup(s => s.GenerateReceiptRelativePath(paymentWithoutPath.RequestId))
                               .Returns(generatedPath);

            // Act
            var result = await _paymentService.GetReceiptAsync(paymentId);

            // Assert
            Assert.Equal(string.Empty, result);

            // Verify it did try to generate and save it before failing on the 3rd fetch
            _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(paymentWithoutPath), Times.Once);
            _mockReceiptService.Verify(s => s.GetReceiptDocument(It.IsAny<Payment>()), Times.Never);
        }

        #endregion

        /// <summary>
        /// Concrete implementation of the abstract PaymentService for testing purposes.
        /// </summary>
        private class TestablePaymentService : PaymentService
        {
            public TestablePaymentService(IPaymentRepository paymentRepository, IReceiptService receiptService)
                : base(paymentRepository, receiptService)
            {
            }
        }
    }
}
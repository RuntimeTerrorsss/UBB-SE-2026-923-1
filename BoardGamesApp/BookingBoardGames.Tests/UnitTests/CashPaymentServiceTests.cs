using System;
using System.Threading.Tasks;
using BookingBoardGames.Data.Constants;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Mapper;
using BookingBoardGames.Sharing.Services;
using Moq;
using Xunit;

namespace BookingBoardGames.Tests.Services
{
    public class CashPaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
        private readonly Mock<ICashPaymentMapper> _mockCashPaymentMapper;
        private readonly Mock<IReceiptService> _mockReceiptService;
        private readonly CashPaymentService _cashPaymentService;

        public CashPaymentServiceTests()
        {
            _mockPaymentRepository = new Mock<IPaymentRepository>();
            _mockCashPaymentMapper = new Mock<ICashPaymentMapper>();
            _mockReceiptService = new Mock<IReceiptService>();

            _cashPaymentService = new CashPaymentService(
                _mockPaymentRepository.Object,
                _mockCashPaymentMapper.Object,
                _mockReceiptService.Object);
        }

        #region AddCashPaymentAsync

        [Fact]
        public async Task AddCashPaymentAsync_ValidData_ReturnsPaymentIdentifier()
        {
            // Arrange
            var dto = new CashPaymentDataTransferObject(0, 100, 2, 3, 50.0m);
            var paymentEntity = new Payment();
            int expectedIdentifier = 10;

            _mockCashPaymentMapper.Setup(m => m.TurnDataTransferObjectIntoEntity(dto))
                                  .Returns(paymentEntity);

            _mockPaymentRepository.Setup(r => r.AddPaymentAsync(paymentEntity))
                                  .ReturnsAsync(expectedIdentifier);

            // Act
            var result = await _cashPaymentService.AddCashPaymentAsync(dto);

            // Assert
            Assert.Equal(expectedIdentifier, result);
            Assert.Equal("CASH", paymentEntity.PaymentMethod);
            Assert.Equal(PaymentConstrants.StateCompleted, paymentEntity.PaymentState);
            _mockPaymentRepository.Verify(r => r.AddPaymentAsync(paymentEntity), Times.Once);
        }

        #endregion

        #region GetCashPaymentAsync

        [Fact]
        public async Task GetCashPaymentAsync_ValidIdentifier_ReturnsMappedDTO()
        {
            // Arrange
            int paymentId = 1;
            var paymentEntity = new Payment();
            var expectedDto = new CashPaymentDataTransferObject(0, 100, 2, 3, 50.0m);

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(paymentEntity);

            _mockCashPaymentMapper.Setup(m => m.TurnEntityIntoDataTransferObject(paymentEntity))
                                  .Returns(expectedDto);

            // Act
            var result = await _cashPaymentService.GetCashPaymentAsync(paymentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto, result);
        }

        #endregion

        #region ConfirmDeliveryAsync

        [Fact]
        public async Task ConfirmDeliveryAsync_AllConfirmed_GeneratesReceiptAndUpdates()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment
            {
                RequestId = 100,
                DateConfirmedSeller = DateTime.Now, // Already confirmed by seller
                DateConfirmedBuyer = null
            };
            string expectedReceiptPath = "/receipts/100.pdf";

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment); // Will be returned for both Get calls

            _mockReceiptService.Setup(s => s.GenerateReceiptRelativePath(payment.RequestId))
                               .Returns(expectedReceiptPath);

            // Act
            await _cashPaymentService.ConfirmDeliveryAsync(paymentId);

            // Assert
            Assert.NotNull(payment.DateConfirmedBuyer);
            Assert.Equal(expectedReceiptPath, payment.ReceiptFilePath);
            Assert.Equal(PaymentConstrants.StateConfirmed, payment.PaymentState); // Set by IsAllConfirmedAsync
            _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(payment), Times.Once);
            _mockReceiptService.Verify(s => s.GenerateReceiptRelativePath(payment.RequestId), Times.Once);
        }

        [Fact]
        public async Task ConfirmDeliveryAsync_SellerNotConfirmed_DoesNotGenerateReceipt()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment
            {
                RequestId = 100,
                DateConfirmedSeller = null, // Not confirmed by seller
                DateConfirmedBuyer = null
            };

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            // Act
            await _cashPaymentService.ConfirmDeliveryAsync(paymentId);

            // Assert
            Assert.NotNull(payment.DateConfirmedBuyer);
            Assert.Null(payment.ReceiptFilePath);
            _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(payment), Times.Once);
            _mockReceiptService.Verify(s => s.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region ConfirmPaymentAsync

        [Fact]
        public async Task ConfirmPaymentAsync_AllConfirmed_GeneratesReceiptAndUpdates()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment
            {
                RequestId = 100,
                DateConfirmedBuyer = DateTime.Now, // Already confirmed by buyer
                DateConfirmedSeller = null
            };
            string expectedReceiptPath = "/receipts/100.pdf";

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            _mockReceiptService.Setup(s => s.GenerateReceiptRelativePath(payment.RequestId))
                               .Returns(expectedReceiptPath);

            // Act
            await _cashPaymentService.ConfirmPaymentAsync(paymentId);

            // Assert
            Assert.NotNull(payment.DateConfirmedSeller);
            Assert.Equal(expectedReceiptPath, payment.ReceiptFilePath);
            Assert.Equal(PaymentConstrants.StateConfirmed, payment.PaymentState);
            _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(payment), Times.Once);
            _mockReceiptService.Verify(s => s.GenerateReceiptRelativePath(payment.RequestId), Times.Once);
        }

        [Fact]
        public async Task ConfirmPaymentAsync_BuyerNotConfirmed_DoesNotGenerateReceipt()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment
            {
                RequestId = 100,
                DateConfirmedBuyer = null, // Not confirmed by buyer
                DateConfirmedSeller = null
            };

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            // Act
            await _cashPaymentService.ConfirmPaymentAsync(paymentId);

            // Assert
            Assert.NotNull(payment.DateConfirmedSeller);
            Assert.Null(payment.ReceiptFilePath);
            _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(payment), Times.Once);
            _mockReceiptService.Verify(s => s.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region IsAllConfirmedAsync

        [Fact]
        public async Task IsAllConfirmedAsync_BothConfirmed_ReturnsTrueAndSetsState()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment
            {
                DateConfirmedSeller = DateTime.Now,
                DateConfirmedBuyer = DateTime.Now,
                PaymentState = PaymentConstrants.StateCompleted
            };

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            // Act
            var result = await _cashPaymentService.IsAllConfirmedAsync(paymentId);

            // Assert
            Assert.True(result);
            Assert.Equal(PaymentConstrants.StateConfirmed, payment.PaymentState);
        }

        [Fact]
        public async Task IsAllConfirmedAsync_MissingConfirmation_ReturnsFalse()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment
            {
                DateConfirmedSeller = DateTime.Now,
                DateConfirmedBuyer = null
            };

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            // Act
            var result = await _cashPaymentService.IsAllConfirmedAsync(paymentId);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsDeliveryConfirmedAsync

        [Fact]
        public async Task IsDeliveryConfirmedAsync_BuyerConfirmed_ReturnsTrue()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment { DateConfirmedBuyer = DateTime.Now };

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            // Act
            var result = await _cashPaymentService.IsDeliveryConfirmedAsync(paymentId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsDeliveryConfirmedAsync_BuyerNotConfirmed_ReturnsFalse()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment { DateConfirmedBuyer = null };

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            // Act
            var result = await _cashPaymentService.IsDeliveryConfirmedAsync(paymentId);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsPaymentConfirmedAsync

        [Fact]
        public async Task IsPaymentConfirmedAsync_SellerConfirmed_ReturnsTrue()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment { DateConfirmedSeller = DateTime.Now };

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            // Act
            var result = await _cashPaymentService.IsPaymentConfirmedAsync(paymentId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsPaymentConfirmedAsync_SellerNotConfirmed_ReturnsFalse()
        {
            // Arrange
            int paymentId = 1;
            var payment = new Payment { DateConfirmedSeller = null };

            _mockPaymentRepository.Setup(r => r.GetPaymentByIdentifierAsync(paymentId))
                                  .ReturnsAsync(payment);

            // Act
            var result = await _cashPaymentService.IsPaymentConfirmedAsync(paymentId);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
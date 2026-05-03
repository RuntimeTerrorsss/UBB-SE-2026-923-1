using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Enum;
using BookingBoardGames.Src.Shared;
using BookingBoardGames.Src.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Payments;

namespace BookingBoardGames.Tests.Receipt
{
    public class ReceiptServiceTests
    {
        private Mock<IUserRepository> userRepositoryMock;
        private Mock<IGameRepository> gameRepositoryMock;
        private Mock<IRequestService> requestServiceMock;
        private ReceiptService receiptService;

        private void InitializeService()
        {
            userRepositoryMock = new Mock<IUserRepository>();
            gameRepositoryMock = new Mock<IGameRepository>();
            requestServiceMock = new Mock<IRequestService>();

            userRepositoryMock
                .Setup(repository => repository.GetById(It.IsAny<int>()))
                .Returns((int userIdToSearch) => new User(userIdToSearch, $"user_{userIdToSearch}", "country", "city", "street", "number"));

            gameRepositoryMock
                .Setup(repository => repository.GetById(It.IsAny<int>()))
                .Returns((int gameIdToSearch) => new Game(gameIdToSearch, $"game_{gameIdToSearch}", 100m));

            requestServiceMock
                .Setup(service => service.GetRequestById(It.IsAny<int>()))
                .Returns((int requestIdToSearch) => new Request(requestIdToSearch, 1, 2, 3, DateTime.Now, DateTime.Now.AddDays(3)));

            receiptService = new ReceiptService(
                userRepositoryMock.Object,
                requestServiceMock.Object,
                gameRepositoryMock.Object
            );
        }

        private static string ToFullPath(string relativePath)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "BookingBoardGames",
                relativePath.TrimStart('\\', '/'));
        }

        private Payment MakePayment(string relativePath, string paymentMethod)
        {
            return new Payment
            {
                ReceiptFilePath = relativePath,
                RequestId = 1,
                ClientId = 1,
                OwnerId = 2,
                PaymentMethod = paymentMethod,
                PaidAmount = 100,
                DateOfTransaction = DateTime.Now
            };
        }

        // ================================ GenerateReceiptRelativePath ======================================
        [Fact]
        public void GenerateReceiptRelativePath_WhenCalled_ReturnsPathFolder()
        {
            InitializeService();

            var receiptPath = receiptService.GenerateReceiptRelativePath(1);
            Assert.StartsWith("receipts\\", receiptPath);
        }

        [Fact]
        public void GenerateReceiptRelativePath_WhenCalled_ReturnsPathEndingWithPdf()
        {
            InitializeService();
            var receiptPath = receiptService.GenerateReceiptRelativePath(1);
            Assert.EndsWith(".pdf", receiptPath);
        }

        [Fact]
        public void GenerateReceiptRelativePath_WhenCalled_ContainsRequestId()
        {
            InitializeService();
            var receiptPath = receiptService.GenerateReceiptRelativePath(1);
            Assert.Contains("1", receiptPath);
        }

        [Fact]
        public void GenerateReceiptRelativePath_SameIds_DifferentPaths()
        {
            InitializeService();
            var receiptPath = receiptService.GenerateReceiptRelativePath(1);
            Thread.Sleep(1000);
            var receiptPathAfter1Second = receiptService.GenerateReceiptRelativePath(1);

            Assert.NotEqual(receiptPath, receiptPathAfter1Second);
        }

        // ================================ GetReceiptDocument ======================================
        [Fact]
        public void GetReceiptDocument_NullFilePath_ThrowsException()
        {
            InitializeService();
            var payment = new Payment { ReceiptFilePath = null };
            Assert.Throws<InvalidOperationException>(() => receiptService.GetReceiptDocument(payment));
        }

        [Fact]
        public void GetReceiptDocument_EmptyFilePath_ThrowsException()
        {
            InitializeService();
            var payment = new Payment { ReceiptFilePath = string.Empty };
            Assert.Throws<InvalidOperationException>(() => receiptService.GetReceiptDocument(payment));
        }

        [Fact]
        public void GetReceiptDocument_FileExists_ReturnsPath()
        {
            InitializeService();
            string relativePath = $"receipts\\receipt_1_{DateTime.Now:yyMMdd_HHmmss}.pdf";
            string fullPath = ToFullPath(relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllBytes(fullPath, new byte[] { 0x25, 0x50, 0x44, 0x46 });

            var payment = new Payment { ReceiptFilePath = relativePath };
            var returnedPath = receiptService.GetReceiptDocument(payment);

            Assert.Equal(fullPath, returnedPath);

            // clean
            File.Delete(fullPath);
        }

        [Fact]
        public void GetReceiptDocument_InexistentFileCardPayment_CreatesPdfAndReturnsPath()
        {
            InitializeService();
            string relativePath = $"receipts\\receipt_1_{DateTime.Now:yyMMdd_HHmmss}.pdf";
            string fullPath = ToFullPath(relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            var payment = MakePayment(relativePath, "card");
            var returnedPath = receiptService.GetReceiptDocument(payment);

            Assert.True(File.Exists(returnedPath));
            Assert.EndsWith(".pdf", returnedPath);

            // clean
            File.Delete(returnedPath);
        }

        [Fact]
        public void GetReceiptDocument_InexistentFileCashPayment_CreatesPdfAndReturnsPath()
        {
            InitializeService();
            string relativePath = $"receipts\\receipt_1_{DateTime.Now:yyMMdd_HHmmss}.pdf";
            string fullPath = ToFullPath(relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            var payment = MakePayment(relativePath, "cash");
            var returnedPath = receiptService.GetReceiptDocument(payment);
            Assert.True(File.Exists(returnedPath));

            // clean
            File.Delete(returnedPath);
        }

        [Fact]
        public void GetReceiptDocument_InvalidFilename_FallsBackToTodayDate()
        {
            InitializeService();
            string relativePath = "receipts\\receipt_BADNAME.pdf";
            string fullPath = ToFullPath(relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            var payment = MakePayment(relativePath, "card");
            var returnedPath = receiptService.GetReceiptDocument(payment);

            Assert.True(File.Exists(returnedPath));

            // clean
            File.Delete(returnedPath);
        }
    }
}

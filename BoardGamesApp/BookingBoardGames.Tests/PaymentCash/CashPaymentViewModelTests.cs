using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Data.Services;
using BookingBoardGames.Data.DTO;
using BookingBoardGames.Data.Services;
using BookingBoardGames.Data.ViewModels;
using Moq;

namespace BookingBoardGames.Tests.PaymentCash
{
    public class CashPaymentViewModelTests
    {
        [Fact]
        public void Constructor_SetsSummaryFromRequestGameAndUsers()
        {
            var RequestId = 100;
            var messageId = 77;
            var delivery = "22B";
            var start = new DateTime(2026, 5, 1);
            var end = new DateTime(2026, 5, 5);
            var Rental = new Rental(RequestId, gameId: 200, clientId: 301, ownerId: 302, start, end);
            var game = new Game(12m, 2, 4, "Description", 1, 2, 4, "Description", 1);
            var client = new User(301, "renter", "R", "Ro", "Clooj", "Low", "22B", string.Empty, 0m);
            var owner = new User(302, "lender", "L", "Ro", "Valcea", "High", "1", string.Empty, 0m);

            var RentalService = new Mock<IRentalService>();
            RentalService.Setup(RentalServiceDependency => RentalServiceDependency.GetRentalById(RequestId)).Returns(Rental);
            RentalService.Setup(RentalServiceDependency => RentalServiceDependency.GetRentalPrice(RequestId)).Returns(88.5m);
            var GamesRepository = new Mock<InterfaceGamesRepository>();
            GamesRepository.Setup(GamesRepositoryDependency => GamesRepositoryDependency.GetGameById(200)).Returns(game);
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(userRepositoryDependency => userRepositoryDependency.GetById(301)).Returns(client);
            userRepository.Setup(userRepositoryDependency => userRepositoryDependency.GetById(302)).Returns(owner);
            var cashPaymentService = new Mock<ICashPaymentService>();
            cashPaymentService.Setup(cashPaymentServiceDependency => cashPaymentServiceDependency.AddCashPayment(It.IsAny<CashPaymentDataTransferObject>())).Returns(999);
            var conversationRepository = new Mock<IConversationRepository>();
            var conversationUserRepository = new Mock<IUserRepository>();
            var conversationService = new ConversationService(
                conversationRepository.Object,
                userIdInput: 1,
                conversationUserRepository.Object);

            var viewModel = new CashPaymentViewModel(
                cashPaymentService.Object,
                userRepository.Object,
                RentalService.Object,
                GamesRepository.Object,
                RequestId,
                delivery,
                messageId,
                conversationService);

            var expected = new
            {
                OwnerName = "lender",
                GameName = "Description",
                DeliveryAddress = delivery,
            };
            var actual = new
            {
                viewModel.OwnerName,
                viewModel.GameName,
                viewModel.DeliveryAddress,
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Constructor_CreatesCashPaymentUsingRequestParticipantsAndQuotedPrice()
        {
            var RequestId = 50;
            var messageId = 12;
            var Rental = new Rental(RequestId, 1, clientId: 10, ownerId: 20, DateTime.Now, DateTime.Now.AddDays(1));
            var RentalService = new Mock<IRentalService>();
            RentalService.Setup(RentalServiceDependency => RentalServiceDependency.GetRentalById(RequestId)).Returns(Rental);
            RentalService.Setup(RentalServiceDependency => RentalServiceDependency.GetRentalPrice(RequestId)).Returns(40m);
            var GamesRepository = new Mock<InterfaceGamesRepository>();
            GamesRepository.Setup(GamesRepositoryDependency => GamesRepositoryDependency.GetGameById(1)).Returns(new Game(1m, 2, 4, "Description", 1, 2, 4, "Description", 1));
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(userRepositoryDependency => userRepositoryDependency.GetById(10)).Returns(new User(10, "a", "b", "c", "d", "e", "f", string.Empty, 0m));
            userRepository.Setup(userRepositoryDependency => userRepositoryDependency.GetById(20)).Returns(new User(20, "b", "b", "c", "d", "e", "f", string.Empty, 0m));
            var cashPaymentService = new Mock<ICashPaymentService>();
            CashPaymentDataTransferObject? addedCashPaymentDataTransferObject = null;
            cashPaymentService.Setup(cashPaymentServiceDependency => cashPaymentServiceDependency.AddCashPayment(It.IsAny<CashPaymentDataTransferObject>()))
                .Callback<CashPaymentDataTransferObject>(cashPaymentDataTransferObject => addedCashPaymentDataTransferObject = cashPaymentDataTransferObject)
                .Returns(1);
            var conversationRepository = new Mock<IConversationRepository>();
            var conversationUserRepository = new Mock<IUserRepository>();
            var conversationService = new ConversationService(
                conversationRepository.Object,
                userIdInput: 1,
                conversationUserRepository.Object);

            var viewModel = new CashPaymentViewModel(
                cashPaymentService.Object,
                userRepository.Object,
                RentalService.Object,
                GamesRepository.Object,
                RequestId,
                "address",
                messageId,
                conversationService);

            Assert.NotNull(addedCashPaymentDataTransferObject);
            var expected = new
            {
                Id = -1,
                RequestId = RequestId,
                ClientId = 10,
                OwnerId = 20,
                PaidAmount = 40m,
            };
            var actual = new
            {
                addedCashPaymentDataTransferObject!.Id,
                addedCashPaymentDataTransferObject.RequestId,
                addedCashPaymentDataTransferObject.ClientId,
                addedCashPaymentDataTransferObject.OwnerId,
                addedCashPaymentDataTransferObject.PaidAmount,
            };

            Assert.Equal(expected, actual);
            Assert.NotNull(viewModel);
        }

        [Fact]
        public void Constructor_FinalizesRentalRequestAndCreatesCashAgreementMessage()
        {
            var RequestId = 60;
            var messageId = 33;
            var returnedPaymentId = 555;
            var Rental = new Rental(RequestId, 1, 40, 50, DateTime.Now, DateTime.Now.AddDays(1));
            var RentalService = new Mock<IRentalService>();
            RentalService.Setup(RentalServiceDependency => RentalServiceDependency.GetRentalById(RequestId)).Returns(Rental);
            RentalService.Setup(RentalServiceDependency => RentalServiceDependency.GetRentalPrice(RequestId)).Returns(1m);
            var GamesRepository = new Mock<InterfaceGamesRepository>();
            GamesRepository.Setup(GamesRepositoryDependency => GamesRepositoryDependency.GetGameById(1)).Returns(new Game(1m, 2, 4, "Description", 1, 2, 4, "Description", 1));
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(userRepositoryDependency => userRepositoryDependency.GetById(40)).Returns(new User(40, "a", "b", "c", "d", "e", "f", string.Empty, 0m));
            userRepository.Setup(userRepositoryDependency => userRepositoryDependency.GetById(50)).Returns(new User(50, "b", "b", "c", "d", "e", "f", string.Empty, 0m));
            var cashPaymentService = new Mock<ICashPaymentService>();
            cashPaymentService.Setup(cashPaymentServiceDependency => cashPaymentServiceDependency.AddCashPayment(It.IsAny<CashPaymentDataTransferObject>())).Returns(returnedPaymentId);
            var conversationRepository = new Mock<IConversationRepository>();
            var conversationUserRepository = new Mock<IUserRepository>();
            var conversationService = new ConversationService(
                conversationRepository.Object,
                userIdInput: 1,
                conversationUserRepository.Object);

            var viewModel = new CashPaymentViewModel(
                cashPaymentService.Object,
                userRepository.Object,
                RentalService.Object,
                GamesRepository.Object,
                RequestId,
                "address",
                messageId,
                conversationService);

            conversationRepository.Verify(conversationRepositoryService => conversationRepositoryService.HandleRentalRequestFinalization(messageId), Times.Once);
            conversationRepository.Verify(conversationRepositoryService => conversationRepositoryService.CreateCashAgreementMessage(messageId, returnedPaymentId), Times.Once);
            Assert.NotNull(viewModel);
        }
    }
}






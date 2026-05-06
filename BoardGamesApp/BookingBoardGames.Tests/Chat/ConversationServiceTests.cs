using BookingBoardGames.Src.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;
using BookingBoardGames.Src.Enum;
using Moq;
using Xunit;

namespace BookingBoardGames.Tests.Chat
{
    public class ConversationServiceTests
    {
        private readonly Mock<IConversationRepository> conversationRepositoryMock;
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly ConversationService conversationService;

        public ConversationServiceTests()
        {
            int currentUserId = 1;
            decimal defaultBalance = 0;
            string testDisplayName = "display";
            string testCountry = "RO";
            string testCity = "Sibiu";

            conversationRepositoryMock = new Mock<IConversationRepository>();
            conversationRepositoryMock.Setup(r => r.GetParticipantUserIds(It.IsAny<int>())).Returns(new List<int>());
            userRepositoryMock = new Mock<IUserRepository>();

            userRepositoryMock
                .Setup(userRepository => userRepository.GetById(It.IsAny<int>()))
                .Returns((int userIdentifier) => new User(
                    "user" + userIdentifier,
                    testDisplayName,
                    "email@test.com",
                    "hash",
                    testCity,
                    testCountry) { Id = userIdentifier, Balance = defaultBalance });

            conversationService = new ConversationService(
                conversationRepositoryMock.Object,
                currentUserId,
                userRepositoryMock.Object);
        }

        private MessageDataTransferObject CreateTextDTO()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            int missingIdentifier = -1;
            string textContent = "hello";

            return new MessageDataTransferObject(
                defaultMessageId,
                targetConversationId,
                senderIdentifier,
                receiverIdentifier,
                DateTime.Now,
                textContent,
                MessageType.MessageText,
                string.Empty,
                false,
                false,
                false,
                false,
                missingIdentifier,
                missingIdentifier);
        }

        [Fact]
        public void FetchConversations_EmptyRepository_ReturnsEmptyList()
        {
            conversationRepositoryMock.Setup(repository => repository.GetConversationsForUser(It.IsAny<int>()))
                     .Returns(new List<Conversation>());

            var resultList = conversationService.FetchConversations();

            Assert.Empty(resultList);
        }

        [Fact]
        public void FetchConversations_ValidRepository_ReturnsMappedConversations()
        {
            int targetConversationId = 1;
            int firstParticipantId = 1;
            int secondParticipantId = 2;

            var participants = new List<ConversationParticipant>
            {
                new ConversationParticipant(targetConversationId, firstParticipantId),
                new ConversationParticipant(targetConversationId, secondParticipantId)
            };

            var testConversation = new Conversation(participants)
            {
                ConversationId = targetConversationId
            };

            conversationRepositoryMock.Setup(repository => repository.GetConversationsForUser(firstParticipantId))        
                     .Returns(new List<Conversation> { testConversation });

            var resultList = conversationService.FetchConversations();

            Assert.Single(resultList);
            Assert.Equal(targetConversationId, resultList.First().Id);
        }

        [Fact]
        public void SendMessage_ValidInput_CallsRepositoryHandleNewMessage()
        {
            var messageDataTransferObject = CreateTextDTO();
            var message = new TextMessage { ConversationId = 1, MessageId = 1, Conversation = null!, Sender = null!, Receiver = null! };

            conversationRepositoryMock.Setup(repository => repository.HandleNewMessage(It.IsAny<Message>())).Returns(message);

            conversationService.SendMessage(messageDataTransferObject);

            conversationRepositoryMock.Verify(repository => repository.HandleNewMessage(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void UpdateMessage_ValidInput_CallsRepositoryHandleMessageUpdate()
        {
            var messageDataTransferObject = CreateTextDTO();

            conversationRepositoryMock.Setup(repository => repository.HandleMessageUpdate(It.IsAny<Message>()));

            conversationService.UpdateMessage(messageDataTransferObject);

            conversationRepositoryMock.Verify(repository => repository.HandleMessageUpdate(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void SendReadReceipt_ValidConversation_CallsRepositoryHandleReadReceipt()
        {
            int targetConversationId = 1;
            int firstParticipantId = 1;
            int secondParticipantId = 2;

            var participants = new List<ConversationParticipant>
            {
                new ConversationParticipant(targetConversationId, firstParticipantId),
                new ConversationParticipant(targetConversationId, secondParticipantId)
            };

            var conversationDataTransferObject = new ConversationDTO(
                targetConversationId,
                participants,
                new List<MessageDataTransferObject>(),
                new Dictionary<int, DateTime>
                {
                    { firstParticipantId, DateTime.Now },
                    { secondParticipantId, DateTime.Now }
                });

            conversationRepositoryMock.Setup(repository => repository.HandleReadReceipt(It.IsAny<ReadReceiptDTO>()));        

            conversationService.SendReadReceipt(conversationDataTransferObject);

            conversationRepositoryMock.Verify(repository => repository.HandleReadReceipt(It.IsAny<ReadReceiptDTO>()), Times.Once);
        }

        [Fact]
        public void SendReadReceipt_ValidConversation_SelectsOtherParticipantCorrectly()
        {
            int targetConversationId = 1;
            int currentUserId = 1;
            int externalUserId = 2;

            var participants = new List<ConversationParticipant>
            {
                new ConversationParticipant(targetConversationId, currentUserId),
                new ConversationParticipant(targetConversationId, externalUserId)
            };

            var conversationDataTransferObject = new ConversationDTO(
                targetConversationId,
                participants,
                new List<MessageDataTransferObject>(),
                new Dictionary<int, DateTime>
                {
                    { currentUserId, DateTime.Now },
                    { externalUserId, DateTime.Now }
                });

            ReadReceiptDTO capturedReceipt = null;

            conversationRepositoryMock
                .Setup(repository => repository.HandleReadReceipt(It.IsAny<ReadReceiptDTO>()))
                .Callback<ReadReceiptDTO>(receiptObject => capturedReceipt = receiptObject);

            conversationService.SendReadReceipt(conversationDataTransferObject);

            Assert.Equal(currentUserId, capturedReceipt.ReaderId);
            Assert.Equal(externalUserId, capturedReceipt.ReceiverId);
        }

        [Fact]
        public void MessageToDTO_TextMessage_MapsCorrectly()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string textContent = "hello";

            var textMessage = new TextMessage(targetConversationId, senderIdentifier, receiverIdentifier, textContent)
            {
                MessageId = defaultMessageId,
                MessageSentTime = DateTime.Now,
                Conversation = null!,
                Sender = null!,
                Receiver = null!
            };

            var messageDataTransferObject = conversationService.MessageToMessageDTO(textMessage);

            Assert.Equal(MessageType.MessageText, messageDataTransferObject.Type);
            Assert.Equal(textContent, messageDataTransferObject.Content);
        }

        [Fact]
        public void MessageDTOToMessage_TextMessage_MapsCorrectly()
        {
            var messageDataTransferObject = CreateTextDTO();

            var domainMessage = conversationService.MessageDTOToMessage(messageDataTransferObject);

            Assert.IsType<TextMessage>(domainMessage);
        }

        [Fact]
        public void OnMessageReceived_ValidMessage_TriggersActionMessageProcessedEvent()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string textContent = "hi";

            var newTextMessage = new TextMessage(targetConversationId, senderIdentifier, receiverIdentifier, textContent)
            {
                MessageId = defaultMessageId,
                MessageSentTime = DateTime.Now,
                Conversation = null!,
                Sender = null!,
                Receiver = null!
            };

            bool eventInvoked = false;

            conversationService.ActionMessageProcessed += (messageDataTransferObject, senderName) => eventInvoked = true;

            conversationService.OnMessageReceived(newTextMessage);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void OnConversationReceived_ValidConversation_TriggersActionConversationProcessedEvent()
        {
            int targetConversationId = 1;
            int firstParticipantId = 1;
            int secondParticipantId = 2;

            var participants = new List<ConversationParticipant>
            {
                new ConversationParticipant(targetConversationId, firstParticipantId),
                new ConversationParticipant(targetConversationId, secondParticipantId)
            };

            var testConversation = new Conversation(participants)
            {
                ConversationId = targetConversationId
            };

            bool eventInvoked = false;

            conversationService.ActionConversationProcessed += (conversationDataTransferObject, senderName) => eventInvoked = true;

            conversationService.OnConversationReceived(testConversation);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void OnReadReceiptReceived_ValidReceipt_TriggersActionReadReceiptProcessedEvent()
        {
            int targetConversationId = 1;
            int readerIdentifier = 1;
            int receiverIdentifier = 2;

            var testReadReceipt = new ReadReceiptDTO(targetConversationId, readerIdentifier, receiverIdentifier, DateTime.Now);

            bool eventInvoked = false;

            conversationService.ActionReadReceiptProcessed += (receiptDataTransferObject) => eventInvoked = true;

            conversationService.OnReadReceiptReceived(testReadReceipt);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void OnMessageUpdateReceived_ValidUpdate_TriggersActionMessageUpdateProcessedEvent()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string textContent = "hi";

            var updatedMessage = new TextMessage(targetConversationId, senderIdentifier, receiverIdentifier, textContent)
            {
                MessageId = defaultMessageId,
                MessageSentTime = DateTime.Now,
                Conversation = null!,
                Sender = null!,
                Receiver = null!
            };

            bool eventInvoked = false;

            conversationService.ActionMessageUpdateProcessed += (messageDataTransferObject, senderName) => eventInvoked = true;

            conversationService.OnMessageUpdateReceived(updatedMessage);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void OnCardPaymentSelected_ValidCall_CallsHandleRentalRequestFinalizationOnly()
        {
            int testMessageId = 10;

            conversationRepositoryMock
                .Setup(repository => repository.HandleRentalRequestFinalization(It.IsAny<int>()));

            conversationService.OnCardPaymentSelected(testMessageId);

            conversationRepositoryMock.Verify(repository =>
                repository.HandleRentalRequestFinalization(testMessageId),
                Times.Once);
        }

        [Fact]
        public void OnCashPaymentSelected_ValidCall_CallsHandleRentalRequestFinalizationAndCreateCashAgreement()
        {
            int testMessageId = 10;
            int testPaymentId = 99;

            conversationRepositoryMock
                .Setup(repository => repository.HandleRentalRequestFinalization(It.IsAny<int>()));

            conversationRepositoryMock
                .Setup(repository => repository.CreateCashAgreementMessage(It.IsAny<int>(), It.IsAny<int>()));

            conversationService.OnCashPaymentSelected(testMessageId, testPaymentId);

            conversationRepositoryMock.Verify(repository =>
                repository.HandleRentalRequestFinalization(testMessageId),
                Times.Once);

            conversationRepositoryMock.Verify(repository =>
                repository.CreateCashAgreementMessage(testMessageId, testPaymentId),
                Times.Once);
        }

        [Fact]
        public void GetOtherUserName_MissingUser_ReturnsUnknownUser()
        {
            string expectedUnknownUser = "Unknown User";
            int targetConversationId = 1;
            int firstParticipantId = 1;
            int secondParticipantId = 2;

            userRepositoryMock
                .Setup(userRepository => userRepository.GetById(It.IsAny<int>()))
                .Returns((User)null);

            var participants = new List<ConversationParticipant>
            {
                new ConversationParticipant(targetConversationId, firstParticipantId),
                new ConversationParticipant(targetConversationId, secondParticipantId)
            };

            var conversationDataTransferObject = new ConversationDTO(
                targetConversationId,
                participants,
                new List<MessageDataTransferObject>(),
                new Dictionary<int, DateTime>
                {
                    { firstParticipantId, DateTime.Now },
                    { secondParticipantId, DateTime.Now }
                });

            var resultName = conversationService.GetOtherUserNameByConversationDTO(conversationDataTransferObject);       

            Assert.Equal(expectedUnknownUser, resultName);
        }

        [Fact]
        public void MessageToDTO_ImageMessage_MapsCorrectly()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string testImageName = "img.png";

            var testImageMessage = new ImageMessage(targetConversationId, senderIdentifier, receiverIdentifier, testImageName)
            {
                MessageId = defaultMessageId,
                MessageSentTime = DateTime.Now,
                Conversation = null!,
                Sender = null!,
                Receiver = null!
            };

            var messageDataTransferObject = conversationService.MessageToMessageDTO(testImageMessage);

            Assert.Equal(MessageType.MessageImage, messageDataTransferObject.Type);
            Assert.Equal(testImageName, messageDataTransferObject.ImageUrl);
        }

        [Fact]
        public void MessageToDTO_CashAgreement_MapsCorrectly()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int sellerIdentifier = 1;
            int buyerIdentifier = 2;
            int testPaymentId = 55;

            var testCashAgreement = new CashAgreementMessage(targetConversationId, sellerIdentifier, buyerIdentifier, testPaymentId)
            {
                MessageId = defaultMessageId,
                MessageSentTime = DateTime.Now,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
                IsCashAgreementResolved = false,
                IsCashAgreementAcceptedByBuyer = true,
                IsCashAgreementAcceptedBySeller = false
            };

            var messageDataTransferObject = conversationService.MessageToMessageDTO(testCashAgreement);

            Assert.Equal(MessageType.MessageCashAgreement, messageDataTransferObject.Type);
            Assert.Equal(testPaymentId, messageDataTransferObject.PaymentId);
        }

        [Fact]
        public void MessageToDTO_RentalRequest_MapsCorrectly()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string textContent = "rent";
            int testRequestId = 99;

            var testRentalRequest = new RentalRequestMessage(targetConversationId, senderIdentifier, receiverIdentifier, testRequestId, textContent)
            {
                MessageId = defaultMessageId,
                MessageSentTime = DateTime.Now,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
                IsRequestResolved = false,
                IsRequestAccepted = true
            };

            var messageDataTransferObject = conversationService.MessageToMessageDTO(testRentalRequest);

            Assert.Equal(MessageType.MessageRentalRequest, messageDataTransferObject.Type);
            Assert.Equal(testRequestId, messageDataTransferObject.RequestId);
        }

        [Fact]
        public void MessageToDTO_SystemMessage_MapsCorrectly()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string systemContent = "system";

            var testSystemMessage = new SystemMessage(targetConversationId, senderIdentifier, receiverIdentifier, systemContent)
            {
                MessageId = defaultMessageId,
                MessageSentTime = DateTime.Now,
                Conversation = null!,
                Sender = null!,
                Receiver = null!
            };

            var messageDataTransferObject = conversationService.MessageToMessageDTO(testSystemMessage);

            Assert.Equal(MessageType.MessageSystem, messageDataTransferObject.Type);
            Assert.Equal(systemContent, messageDataTransferObject.Content);
        }

        [Fact]
        public void GetOtherUserNameByMessageDTO_ValidMessage_ReturnsCorrectUser()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string textContent = "hi";
            int missingIdentifier = -1;
            string expectedResultName = "user2";

            var messageDataTransferObject = new MessageDataTransferObject(
                defaultMessageId, targetConversationId, senderIdentifier, receiverIdentifier, DateTime.Now, textContent,  
                MessageType.MessageText, string.Empty, false, false, false, false, missingIdentifier, missingIdentifier   
            );

            var resultName = conversationService.GetOtherUserNameByMessageDTO(messageDataTransferObject);

            Assert.Equal(expectedResultName, resultName);
        }

        [Fact]
        public void MessageDTOToMessage_ImageMessage_MapsCorrectly()
        {
            string testImageName = "img.png";
            var messageDataTransferObject = CreateTextDTO() with { Type = MessageType.MessageImage, ImageUrl = testImageName };

            var domainMessage = conversationService.MessageDTOToMessage(messageDataTransferObject);

            Assert.IsType<ImageMessage>(domainMessage);
        }

        [Fact]
        public void MessageDTOToMessage_RentalRequest_MapsCorrectly()
        {
            var messageDataTransferObject = CreateTextDTO() with { Type = MessageType.MessageRentalRequest };

            var domainMessage = conversationService.MessageDTOToMessage(messageDataTransferObject);

            Assert.IsType<RentalRequestMessage>(domainMessage);
        }

        [Fact]
        public void MessageDTOToMessage_CashAgreement_MapsCorrectly()
        {
            var messageDataTransferObject = CreateTextDTO() with { Type = MessageType.MessageCashAgreement };

            var domainMessage = conversationService.MessageDTOToMessage(messageDataTransferObject);

            Assert.IsType<CashAgreementMessage>(domainMessage);
        }

        [Fact]
        public void MessageDTOToMessage_SystemMessage_MapsCorrectly()
        {
            var messageDataTransferObject = CreateTextDTO() with { Type = MessageType.MessageSystem };

            var domainMessage = conversationService.MessageDTOToMessage(messageDataTransferObject);

            Assert.IsType<SystemMessage>(domainMessage);
        }

        [Fact]
        public void ConversationToConversationDTO_ValidConversation_MapsCorrectly()
        {
            int targetConversationId = 1;
            int firstParticipantId = 1;
            int secondParticipantId = 2;
            int defaultMessageId = 1;
            string textContent = "hi";

            var participants = new List<ConversationParticipant>
            {
                new ConversationParticipant(targetConversationId, firstParticipantId),
                new ConversationParticipant(targetConversationId, secondParticipantId)
            };

            var testConversation = new Conversation(participants)
            {
                ConversationId = targetConversationId,
                Messages = new List<Message> 
                { 
                    new TextMessage(targetConversationId, firstParticipantId, secondParticipantId, textContent) 
                    { 
                        MessageId = defaultMessageId,
                        MessageSentTime = DateTime.Now,
                        Conversation = null!,
                        Sender = null!,
                        Receiver = null!
                    } 
                }
            };

            var conversationDataTransferObject = conversationService.ConversationToConversationDTO(testConversation);     

            Assert.Equal(targetConversationId, conversationDataTransferObject.Id);
            Assert.Single(conversationDataTransferObject.MessageList);
        }

        [Fact]
        public void FetchConversations_ValidRepository_ReturnsMultipleConversations()
        {
            int firstConversationId = 1;
            int secondConversationId = 2;
            int firstParticipantId = 1;
            int secondParticipantId = 2;
            int thirdParticipantId = 3;
            int expectedConversationCount = 2;

            var participants1 = new List<ConversationParticipant>
            {
                new ConversationParticipant(firstConversationId, firstParticipantId),
                new ConversationParticipant(firstConversationId, secondParticipantId)
            };
            var participants2 = new List<ConversationParticipant>
            {
                new ConversationParticipant(secondConversationId, firstParticipantId),
                new ConversationParticipant(secondConversationId, thirdParticipantId)
            };

            var testConversationList = new List<Conversation>
            {
                new Conversation(participants1) { ConversationId = firstConversationId },
                new Conversation(participants2) { ConversationId = secondConversationId }
            };

            conversationRepositoryMock.Setup(repository => repository.GetConversationsForUser(firstParticipantId)).Returns(testConversationList);

            var resultList = conversationService.FetchConversations();

            Assert.Equal(expectedConversationCount, resultList.Count);
        }
    }
}






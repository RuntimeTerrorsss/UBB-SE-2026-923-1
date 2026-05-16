using System.Collections.Generic;
using BookingBoardGames.Data;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Services;
using Moq;
using Xunit;

namespace BookingBoardGames.Tests.Services
{
    public class ConversationNotifierTests
    {
        private readonly ConversationNotifier _notifier;

        public ConversationNotifierTests()
        {
            _notifier = new ConversationNotifier();
        }

        [Fact]
        public void NotifyMessage_RegisteredUser_ReceivesNotification()
        {
            // Arrange
            int userId = 1;
            var mockObserver = new Mock<IConversationService>();
            var message = new Mock<Message>().Object; // FIXED

            _notifier.Register(userId, mockObserver.Object);

            // Act
            _notifier.NotifyMessage(new[] { userId }, message);

            // Assert
            mockObserver.Verify(o => o.OnMessageReceived(message), Times.Once);
        }

        [Fact]
        public void NotifyMessage_UnregisteredUser_DoesNotReceiveNotification()
        {
            // Arrange
            int userId = 1;
            var mockObserver = new Mock<IConversationService>();
            var message = new Mock<Message>().Object; // FIXED

            _notifier.Register(userId, mockObserver.Object);
            _notifier.Unregister(userId); // Unregister immediately

            // Act
            _notifier.NotifyMessage(new[] { userId }, message);

            // Assert
            mockObserver.Verify(o => o.OnMessageReceived(It.IsAny<Message>()), Times.Never);
        }

        [Fact]
        public void NotifyMessage_MultipleRegisteredUsers_OnlySpecifiedUsersReceiveNotification()
        {
            // Arrange
            var mockObserver1 = new Mock<IConversationService>();
            var mockObserver2 = new Mock<IConversationService>();
            var mockObserver3 = new Mock<IConversationService>();
            var message = new Mock<Message>().Object; // FIXED

            _notifier.Register(1, mockObserver1.Object);
            _notifier.Register(2, mockObserver2.Object);
            _notifier.Register(3, mockObserver3.Object);

            // Act
            // Only notifying users 1 and 3
            _notifier.NotifyMessage(new[] { 1, 3 }, message);

            // Assert
            mockObserver1.Verify(o => o.OnMessageReceived(message), Times.Once);
            mockObserver2.Verify(o => o.OnMessageReceived(It.IsAny<Message>()), Times.Never); // User 2 skipped
            mockObserver3.Verify(o => o.OnMessageReceived(message), Times.Once);
        }

        [Fact]
        public void NotifyMessage_DuplicateUserIdsInList_ReceivesNotificationOnlyOnce()
        {
            // Arrange
            int userId = 1;
            var mockObserver = new Mock<IConversationService>();
            var message = new Mock<Message>().Object; // FIXED

            _notifier.Register(userId, mockObserver.Object);

            // Act
            // Pass the same user ID multiple times (tests the .Distinct() branch in SnapshotSubscribers)
            _notifier.NotifyMessage(new[] { userId, userId, userId }, message);

            // Assert
            mockObserver.Verify(o => o.OnMessageReceived(message), Times.Once);
        }

        [Fact]
        public void NotifyMessageUpdate_RegisteredUsers_ReceiveUpdateNotification()
        {
            // Arrange
            int userId = 1;
            var mockObserver = new Mock<IConversationService>();
            var message = new Mock<Message>().Object; // FIXED

            _notifier.Register(userId, mockObserver.Object);

            // Act
            _notifier.NotifyMessageUpdate(new[] { userId }, message);

            // Assert
            mockObserver.Verify(o => o.OnMessageUpdateReceived(message), Times.Once);
        }

        [Fact]
        public void NotifyReadReceipt_RegisteredUsers_ReceiveReadReceipt()
        {
            // Arrange
            int userId = 1;
            var mockObserver = new Mock<IConversationService>();
            var readReceipt = new ReadReceiptDTO(1, 100, userId, DateTime.UtcNow);

            _notifier.Register(userId, mockObserver.Object);

            // Act
            _notifier.NotifyReadReceipt(new[] { userId }, readReceipt);

            // Assert
            mockObserver.Verify(o => o.OnReadReceiptReceived(readReceipt), Times.Once);
        }

        [Fact]
        public void NotifyNewConversation_RegisteredParticipants_ReceiveConversationNotification()
        {
            // Arrange
            var mockObserver1 = new Mock<IConversationService>();
            var mockObserver2 = new Mock<IConversationService>();

            _notifier.Register(1, mockObserver1.Object);
            _notifier.Register(2, mockObserver2.Object);

            var conversation = new Conversation
            {
                Participants = new List<ConversationParticipant>
        {
            new ConversationParticipant { UserId = 1 },
            new ConversationParticipant { UserId = 2 }
        }
            };

            // Act
            _notifier.NotifyNewConversation(conversation);

            // Assert
            mockObserver1.Verify(o => o.OnConversationReceived(conversation), Times.Once);
            mockObserver2.Verify(o => o.OnConversationReceived(conversation), Times.Once);
        }

        [Fact]
        public void SnapshotSubscribers_UserNotRegistered_GracefullyIgnores()
        {
            // Arrange
            var mockObserver = new Mock<IConversationService>();
            var message = new Mock<Message>().Object; // FIXED

            // Register user 1, but we will notify user 2 (who is not registered)
            _notifier.Register(1, mockObserver.Object);

            // Act
            // TryGetValue branch will return false for User 2
            _notifier.NotifyMessage(new[] { 2 }, message);

            // Assert
            mockObserver.Verify(o => o.OnMessageReceived(It.IsAny<Message>()), Times.Never);
        }
    }
}

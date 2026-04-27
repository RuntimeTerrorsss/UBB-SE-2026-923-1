using System.Collections.Generic;
using BookingBoardGames.Repositories;
using BookingBoardGames.Src.Models;
using BookingBoardGames.Src.Services;

namespace BookingBoardGames.Src.Repositories
{
    public interface IConversationRepository : IRepository<Conversation>
    {
        List<Conversation> GetConversationsForUser(int userId);

        Conversation GetConversationById(int conversationId);

        void HandleNewMessage(Message message);

        void HandleReadReceipt(ReadReceipt readReceipt);

        void HandleMessageUpdate(Message message);

        int CreateConversation(int senderId, int receiverId);

        void HandleRentalRequestFinalization(int messageId);

        void CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId);

        void CreateSystemMessageForCashAgreementFinalization(int conversationId, string legalDocumentFilePath);

        void Subscribe(int userId, IConversationService observer);

        void Unsubscribe(int userId);

        void NotifySubscribersAboutMessage(Message message);

        void NotifySubscribersAboutMessageUpdate(Message message);

        void NotifySubscribersAboutNewConversation(Conversation conversation);

        void NotifySubscribersAboutReadReceipt(ReadReceipt readReceipt);
    }
}

// <copyright file="IConversationRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using BookingBoardGames.Data;
using BookingBoardGames.Src.DTO;

namespace BookingBoardGames.Src.Repositories
{
    public interface IConversationRepository
    {
        List<Conversation> GetConversationsForUser(int userId);

        Conversation GetConversationById(int conversationId);

        IReadOnlyList<int> GetParticipantUserIds(int conversationId);

        Message HandleNewMessage(Message message);

        Message? HandleMessageUpdate(Message message);

        void HandleReadReceipt(ReadReceiptDTO readReceipt);

        int CreateConversation(int senderId, int receiverId);

        Message? HandleRentalRequestFinalization(int messageId);

        Message? CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId);
    }
}

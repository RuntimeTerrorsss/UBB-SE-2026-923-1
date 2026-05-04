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

        int CreateConversation(int senderId, int receiverId);

        void HandleNewMessage(Message message);

        void HandleMessageUpdate(Message message);

        void HandleReadReceipt(ReadReceiptDTO readReceipt);

        void HandleRentalRequestFinalization(int messageId);

        void CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId);
    }
}

// <copyright file="IConversationRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using BookingBoardGames.Data;
using BookingBoardGames.Src.Services;

namespace BookingBoardGames.Src.Repositories
{
    public interface IConversationRepository
    {
        List<Conversation> GetConversationsForUser(int userId);

        Conversation GetConversationById(int conversationId);

        void HandleNewMessage(Message message);

        void HandleMessageUpdate(Message message);

        void HandleReadReceipt(ReadReceipt readReceipt);

        int CreateConversation(int senderId, int receiverId);

        void HandleRentalRequestFinalization(int messageId);

        void CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId);

        void Subscribe(int userId, IConversationService observer);

        void Unsubscribe(int userId);

        void NotifySubscribersAboutMessage(Message message);

        void NotifySubscribersAboutMessageUpdate(Message message);

        void NotifySubscribersAboutNewConversation(Conversation conversation);

        void NotifySubscribersAboutReadReceipt(ReadReceipt readReceipt);
    }
}

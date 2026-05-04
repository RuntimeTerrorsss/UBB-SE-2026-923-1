// <copyright file="IConversationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using BookingBoardGames.Data;
using BookingBoardGames.Src.DTO;

namespace BookingBoardGames.Src.Services
{
    public interface IConversationService
    {
        void OnMessageReceived(Message message);

        void OnMessageUpdateReceived(Message message);

        void OnConversationReceived(Conversation conversation);

        void OnReadReceiptReceived(ReadReceiptDTO readReceipt);

        List<ConversationDTO> FetchConversations();

        string GetOtherUserNameByConversationDTO(ConversationDTO conversation);

        void UpdateMessage(MessageDataTransferObject message);

        void SendMessage(MessageDataTransferObject message);

        void Subscribe(int userId, IConversationService observer);

        void Unsubscribe(int userId);

        void NotifySubscribersAboutMessage(Message message);

        void NotifySubscribersAboutMessageUpdate(Message message);

        void NotifySubscribersAboutNewConversation(Conversation conversation);

        void NotifySubscribersAboutReadReceipt(ReadReceiptDTO readReceipt);

        event Action<MessageDataTransferObject, string> ActionMessageProcessed;

        event Action<ConversationDTO, string> ActionConversationProcessed;

        event Action<ReadReceiptDTO> ActionReadReceiptProcessed;

        event Action<MessageDataTransferObject, string> ActionMessageUpdateProcessed;
    }
}

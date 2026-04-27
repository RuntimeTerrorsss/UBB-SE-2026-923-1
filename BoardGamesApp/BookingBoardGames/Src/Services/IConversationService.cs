// <copyright file="IConversationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Models;

namespace BookingBoardGames.Src.Services
{
    public interface IConversationService
    {
        event Action<MessageDataTransferObject, string> ActionMessageProcessed;

        event Action<ConversationDataTransferObject, string> ActionConversationProcessed;

        event Action<ReadReceiptDataTransferObject> ActionReadReceiptProcessed;

        event Action<MessageDataTransferObject, string> ActionMessageUpdateProcessed;

        void OnMessageReceived(Message message);

        void OnMessageUpdateReceived(Message message);

        void OnConversationReceived(Conversation conversation);

        void OnReadReceiptReceived(ReadReceipt readReceipt);

        List<ConversationDataTransferObject> FetchConversations();

        string GetOtherUserNameByConversationDTO(ConversationDataTransferObject conversation);

        void UpdateMessage(MessageDataTransferObject message);

        void SendMessage(MessageDataTransferObject message);
    }
}

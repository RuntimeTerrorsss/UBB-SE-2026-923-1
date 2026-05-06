// <copyright file="IConversationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingBoardGames.Src.DTO;

namespace BookingBoardGames.Src.Services
{
    public interface IConversationService
    {
        void OnMessageReceived(Message message);

        void OnMessageUpdateReceived(Message message);

        void OnConversationReceived(Conversation conversation);

        void OnReadReceiptReceived(ReadReceiptDTO readReceipt);

        Task<List<ConversationDTO>> FetchConversations();

        string GetOtherUserNameByConversationDTO(ConversationDTO conversation);

        Task UpdateMessage(MessageDataTransferObject message);

        Task SendMessage(MessageDataTransferObject message);

        event Action<MessageDataTransferObject, string> ActionMessageProcessed;

        event Action<ConversationDTO, string> ActionConversationProcessed;

        event Action<ReadReceiptDTO> ActionReadReceiptProcessed;

        event Action<MessageDataTransferObject, string> ActionMessageUpdateProcessed;
    }
}

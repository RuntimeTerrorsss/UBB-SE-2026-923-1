// <copyright file="ConversationAPIProxy.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Src.DTO;

namespace BookingBoardGames.Src.Repositories
{
    public class ConversationAPIProxy : IConversationRepository
    {
        private readonly HttpClient httpClient;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            //Converters = { new MessageJsonConverter() }
        };

        public ConversationAPIProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<Conversation>> GetConversationsForUser(int userId)
        {
            return await this.httpClient.GetFromJsonAsync<List<Conversation>>(
                       $"conversation/user/{userId}", JsonOptions)
                   ?? new List<Conversation>();
        }

        public async Task<Conversation> GetConversationById(int conversationId)
        {
            var response = await this.httpClient.GetAsync($"conversation/{conversationId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Conversation>(JsonOptions)
                   ?? throw new InvalidOperationException($"Conversation {conversationId} was not found.");
        }

        public async Task<IReadOnlyList<int>> GetParticipantUserIds(int conversationId)
        {
            return await this.httpClient.GetFromJsonAsync<List<int>>(
                       $"conversation/{conversationId}/participants", JsonOptions)
                   ?? new List<int>();
        }

        public async Task<int> CreateConversation(int senderId, int receiverId)
        {
            var response = await this.httpClient.PostAsJsonAsync(
                "conversation",
                new { SenderId = senderId, ReceiverId = receiverId },
                JsonOptions);
            response.EnsureSuccessStatusCode();
            var raw = await response.Content.ReadAsStringAsync();
            return int.Parse(raw);
        }

        public async Task<Message> HandleNewMessage(Message message)
        {
            var dto = MessageToDto(message);
            var response = await this.httpClient.PostAsJsonAsync("conversation/messages", dto, JsonOptions);
            response.EnsureSuccessStatusCode();
            var resultDto = await response.Content.ReadFromJsonAsync<MessageDataTransferObject>(JsonOptions)
                            ?? throw new InvalidOperationException("Failed to create message.");
            return DtoToMessage(resultDto);
        }

        public async Task<Message?> HandleMessageUpdate(Message message)
        {
            var dto = MessageToDto(message);
            var response = await this.httpClient.PutAsJsonAsync("conversation/messages", dto, JsonOptions);
            if (!response.IsSuccessStatusCode) return null;
            var resultDto = await response.Content.ReadFromJsonAsync<MessageDataTransferObject>(JsonOptions);
            return resultDto is null ? null : DtoToMessage(resultDto);
        }

        public async Task HandleReadReceipt(ReadReceiptDTO readReceipt)
        {
            var response = await this.httpClient.PostAsJsonAsync(
                "conversation/readreceipt", readReceipt, JsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Message?> HandleRentalRequestFinalization(int messageId)
        {
            var response = await this.httpClient.PostAsync(
                $"conversation/rental/finalize/{messageId}", null);
            if (!response.IsSuccessStatusCode) return null;
            var resultDto = await response.Content.ReadFromJsonAsync<MessageDataTransferObject>(JsonOptions);
            return resultDto is null ? null : DtoToMessage(resultDto);
        }

        public async Task<Message?> CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            var response = await this.httpClient.PostAsync(
                $"conversation/cash/{messageIdOfParentRentalRequestMessage}/{paymentId}", null);
            if (!response.IsSuccessStatusCode) return null;
            var resultDto = await response.Content.ReadFromJsonAsync<MessageDataTransferObject>(JsonOptions);
            return resultDto is null ? null : DtoToMessage(resultDto);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static MessageDataTransferObject MessageToDto(Message message)
        {
            return new MessageDataTransferObject(
                Id: message.MessageId,
                ConversationId: message.ConversationId,
                SenderId: message.MessageSenderId,
                ReceiverId: message.MessageReceiverId,
                SentAt: message.MessageSentTime,
                Content: message.MessageContentAsString ?? string.Empty,
                Type: message switch
                {
                    TextMessage => MessageType.MessageText,
                    ImageMessage => MessageType.MessageImage,
                    RentalRequestMessage => MessageType.MessageRentalRequest,
                    CashAgreementMessage => MessageType.MessageCashAgreement,
                    SystemMessage => MessageType.MessageSystem,
                    _ => throw new ArgumentOutOfRangeException()
                },
                ImageUrl: message is ImageMessage img ? img.MessageImageUrl ?? string.Empty : string.Empty,
                IsResolved: message is RentalRequestMessage r ? r.IsRequestResolved
                          : message is CashAgreementMessage c ? c.IsCashAgreementResolved : false,
                IsAccepted: message is RentalRequestMessage ra ? ra.IsRequestAccepted : false,
                IsAcceptedByBuyer: message is CashAgreementMessage cb ? cb.IsCashAgreementAcceptedByBuyer : false,
                IsAcceptedBySeller: message is CashAgreementMessage cs ? cs.IsCashAgreementAcceptedBySeller : false,
                RequestId: message is RentalRequestMessage rr ? rr.RentalRequestId : -1,
                PaymentId: message is CashAgreementMessage cp ? cp.CashPaymentId : -1
            );
        }

        private static Message DtoToMessage(MessageDataTransferObject dto)
        {
            return dto.Type switch
            {
                MessageType.MessageText => new TextMessage
                {
                    MessageId = dto.Id,
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    TextMessageContent = dto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageImage => new ImageMessage
                {
                    MessageId = dto.Id,
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    MessageImageUrl = dto.ImageUrl,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageRentalRequest => new RentalRequestMessage
                {
                    MessageId = dto.Id,
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    RentalRequestId = dto.RequestId,
                    IsRequestResolved = dto.IsResolved,
                    IsRequestAccepted = dto.IsAccepted,
                    RequestContent = dto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageCashAgreement => new CashAgreementMessage
                {
                    MessageId = dto.Id,
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    CashPaymentId = dto.PaymentId,
                    IsCashAgreementResolved = dto.IsResolved,
                    IsCashAgreementAcceptedByBuyer = dto.IsAcceptedByBuyer,
                    IsCashAgreementAcceptedBySeller = dto.IsAcceptedBySeller,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageSystem => new SystemMessage
                {
                    MessageId = dto.Id,
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    MessageContent = dto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}

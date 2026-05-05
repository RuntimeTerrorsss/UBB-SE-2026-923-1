// <copyright file="ConversationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using BookingBoardGames.Data;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Enum;
using BookingBoardGames.Src.Repositories;

namespace BookingBoardGames.Src.Services
{
    public class ConversationService : IConversationService
    {
        private IConversationNotifier notifier;

        private int UserId { get; set; }

        public event Action<MessageDataTransferObject, string> ActionMessageProcessed;

        public event Action<ConversationDTO, string> ActionConversationProcessed;

        public event Action<ReadReceiptDTO> ActionReadReceiptProcessed;

        public event Action<MessageDataTransferObject, string> ActionMessageUpdateProcessed;

        public ConversationService(IConversationRepository conversationRepo, int userIdInput)
            : this(conversationRepo, userIdInput, App.UserRepository, ResolveNotifier())
        {
        }

        public ConversationService(IConversationRepository conversationRepo, int userIdInput, IUserRepository userRepo)
            : this(conversationRepo, userIdInput, userRepo, ResolveNotifier())
        {
        }

        public ConversationService(IConversationRepository conversationRepo, int userIdInput, IUserRepository userRepo, IConversationNotifier conversationNotifier)
        {
            this.UserId = userIdInput;
            this.notifier = conversationNotifier;

            this.notifier.Register(this.UserId, this);
        }

        private static IConversationNotifier ResolveNotifier()
        {
            return App.ConversationNotifier ?? new ConversationNotifier();
        }

        private void NotifySubscribersAboutMessage(Message message)
        {
            var participants = App.Client.GetFromJsonAsync<List<int>>($"conversation/{message.ConversationId}/participants").GetAwaiter().GetResult();
            this.notifier.NotifyMessage(participants, message);
        }

        private void NotifySubscribersAboutMessageUpdate(Message message)
        {
            var participants = App.Client.GetFromJsonAsync<List<int>>($"conversation/{message.ConversationId}/participants").GetAwaiter().GetResult();
            this.notifier.NotifyMessageUpdate(participants, message);
        }

        private void NotifySubscribersAboutReadReceipt(ReadReceiptDTO readReceipt)
        {
            var participants = App.Client.GetFromJsonAsync<List<int>>($"conversation/{readReceipt.ConversationId}/participants").GetAwaiter().GetResult();
            this.notifier.NotifyReadReceipt(participants, readReceipt);
        }

        private void NotifySubscribersAboutNewConversation(Conversation conversation)
        {
            this.notifier.NotifyNewConversation(conversation);
        }

        public List<ConversationDTO> FetchConversations()
        {
            List<ConversationDTO> conversationList = new List<ConversationDTO>();

            var conversations = App.Client.GetFromJsonAsync<List<Conversation>>($"conversation/user/{this.UserId}").GetAwaiter().GetResult()
                                ?? new List<Conversation>();

            foreach (var conversation in conversations)
            {
                conversationList.Add(this.ConversationToConversationDTO(conversation));
            }

            return conversationList;
        }

        public string GetOtherUserNameByConversationDTO(ConversationDTO conversation)
        {
            int otherUserId = conversation.Participants.First(participantItem => participantItem.UserId != this.UserId).UserId;
            var user = App.Client.GetFromJsonAsync<User>($"users/{otherUserId}").GetAwaiter().GetResult();
            return user?.Username ?? "Unknown User";
        }

        public string GetOtherUserNameByMessageDTO(MessageDataTransferObject message)
        {
            int id = message.SenderId == this.UserId ? message.ReceiverId : message.SenderId;
            var user = App.Client.GetFromJsonAsync<User>($"users/{id}").GetAwaiter().GetResult();
            return user?.Username ?? "Unknown User";
        }

        public void SendMessage(MessageDataTransferObject message)
        {
            var response = App.Client.PostAsJsonAsync("conversation/messages", message).GetAwaiter().GetResult();
            var persistedDto = response.Content.ReadFromJsonAsync<MessageDataTransferObject>().GetAwaiter().GetResult();
            if (persistedDto is null) return;
            var persisted = this.MessageDTOToMessage(persistedDto);
            this.NotifySubscribersAboutMessage(persisted);
        }

        public int CreateConversation(int senderId, int receiverId)
        {
            var response = App.Client.PostAsJsonAsync("conversation", new { SenderId = senderId, ReceiverId = receiverId }).GetAwaiter().GetResult();
            var conversation = response.Content.ReadFromJsonAsync<Conversation>().GetAwaiter().GetResult();
            if (conversation is null) return -1;
            this.NotifySubscribersAboutNewConversation(conversation);
            return conversation.ConversationId;
        }

        public void UpdateMessage(MessageDataTransferObject message)
        {
            var response = App.Client.PutAsJsonAsync("conversation/messages", message).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode) return;
            var persistedDto = response.Content.ReadFromJsonAsync<MessageDataTransferObject>().GetAwaiter().GetResult();
            if (persistedDto is null) return;
            var persisted = this.MessageDTOToMessage(persistedDto);
            this.NotifySubscribersAboutMessageUpdate(persisted);
        }

        public void SendReadReceipt(ConversationDTO conversation)
        {
            var readReceipt = new ReadReceiptDTO(
                conversation.Id,
                this.UserId,
                conversation.Participants.First(participantItem => participantItem.UserId != this.UserId).UserId,
                DateTime.Now);
            App.Client.PostAsJsonAsync("conversation/readreceipt", readReceipt).GetAwaiter().GetResult();
            this.NotifySubscribersAboutReadReceipt(readReceipt);
        }

        public void OnCardPaymentSelected(int messageId)
        {
            this.FinalizeRentalRequest(messageId);
        }

        public void OnCashPaymentSelected(int messageId, int paymentId)
        {
            this.FinalizeRentalRequest(messageId);
            this.SendCashAgreementMessage(messageId, paymentId);
        }

        private void FinalizeRentalRequest(int messageId)
        {
            var response = App.Client.PostAsync($"conversation/rental/finalize/{messageId}", null).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode) return;
            var updatedDto = response.Content.ReadFromJsonAsync<MessageDataTransferObject>().GetAwaiter().GetResult();
            if (updatedDto is null) return;
            var updated = this.MessageDTOToMessage(updatedDto);
            this.NotifySubscribersAboutMessageUpdate(updated);
        }

        private void SendCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            var response = App.Client.PostAsync($"conversation/cash/{messageIdOfParentRentalRequestMessage}/{paymentId}", null).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode) return;
            var createdDto = response.Content.ReadFromJsonAsync<MessageDataTransferObject>().GetAwaiter().GetResult();
            if (createdDto is null) return;
            var created = this.MessageDTOToMessage(createdDto);
            this.NotifySubscribersAboutMessage(created);
        }

        public void OnMessageReceived(Message message)
        {
            MessageDataTransferObject messageDTO = this.MessageToMessageDTO(message);
            string userName = this.GetOtherUserNameByMessageDTO(messageDTO);
            this.ActionMessageProcessed?.Invoke(messageDTO, userName);
        }

        public void OnConversationReceived(Conversation conversation)
        {
            ConversationDTO conversationDTO = this.ConversationToConversationDTO(conversation);
            string userName = this.GetOtherUserNameByConversationDTO(conversationDTO);
            this.ActionConversationProcessed?.Invoke(conversationDTO, userName);
        }

        public void OnReadReceiptReceived(ReadReceiptDTO readReceipt)
        {
            this.ActionReadReceiptProcessed?.Invoke(readReceipt);
        }

        public void OnMessageUpdateReceived(Message message)
        {
            MessageDataTransferObject messageDTO = this.MessageToMessageDTO(message);
            string userName = this.GetOtherUserNameByMessageDTO(messageDTO);
            this.ActionMessageUpdateProcessed?.Invoke(messageDTO, userName);
        }

        public Message MessageDTOToMessage(MessageDataTransferObject messageDto)
        {
            Message toReturn = messageDto.Type switch
            {
                MessageType.MessageText => new TextMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    TextMessageContent = messageDto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageImage => new ImageMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    MessageImageUrl = messageDto.ImageUrl,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageRentalRequest => new RentalRequestMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    RentalRequestId = messageDto.RequestId,
                    IsRequestResolved = messageDto.IsResolved,
                    IsRequestAccepted = messageDto.IsAccepted,
                    RequestContent = messageDto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageCashAgreement => new CashAgreementMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    CashPaymentId = messageDto.PaymentId,
                    IsCashAgreementResolved = messageDto.IsResolved,
                    IsCashAgreementAcceptedByBuyer = messageDto.IsAcceptedByBuyer,
                    IsCashAgreementAcceptedBySeller = messageDto.IsAcceptedBySeller,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageSystem => new SystemMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    MessageContent = messageDto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                _ => throw new ArgumentOutOfRangeException(nameof(messageDto.Type), messageDto.Type, "Unsupported message type."),
            };

            return toReturn;
        }

        public MessageDataTransferObject MessageToMessageDTO(Message message)
        {
            int defaultMissingIdentifier = -1;

            MessageType messageType = message switch
            {
                TextMessage => MessageType.MessageText,
                ImageMessage => MessageType.MessageImage,
                RentalRequestMessage => MessageType.MessageRentalRequest,
                CashAgreementMessage => MessageType.MessageCashAgreement,
                SystemMessage => MessageType.MessageSystem,
                _ => throw new ArgumentOutOfRangeException(nameof(message), message.GetType().Name, "Unknown message subtype."),
            };

            string content = message switch
            {
                TextMessage textMessage => textMessage.TextMessageContent ?? textMessage.MessageContentAsString ?? string.Empty,
                RentalRequestMessage rentalForContent => rentalForContent.RequestContent ?? rentalForContent.MessageContentAsString ?? string.Empty,
                SystemMessage systemMessage => systemMessage.MessageContent ?? systemMessage.MessageContentAsString ?? string.Empty,
                _ => message.MessageContentAsString ?? string.Empty,
            };

            MessageDataTransferObject toReturn = new MessageDataTransferObject(
                Id: message.MessageId,
                ConversationId: message.ConversationId,
                SenderId: message.MessageSenderId,
                ReceiverId: message.MessageReceiverId,
                SentAt: message.MessageSentTime,
                Content: content,
                Type: messageType,
                ImageUrl: message is ImageMessage imageMessage ? imageMessage.MessageImageUrl ?? string.Empty : string.Empty,
                IsResolved: message is RentalRequestMessage rentalResolvedMessage ? rentalResolvedMessage.IsRequestResolved
                          : message is CashAgreementMessage cashResolvedMessage ? cashResolvedMessage.IsCashAgreementResolved
                          : false,
                IsAccepted: message is RentalRequestMessage rentalAcceptedMessage ? rentalAcceptedMessage.IsRequestAccepted : false,
                IsAcceptedByBuyer: message is CashAgreementMessage cashBuyerMessage ? cashBuyerMessage.IsCashAgreementAcceptedByBuyer : false,
                IsAcceptedBySeller: message is CashAgreementMessage cashSellerMessage ? cashSellerMessage.IsCashAgreementAcceptedBySeller : false,
                PaymentId: message is CashAgreementMessage cashPaymentMessage ? cashPaymentMessage.CashPaymentId : defaultMissingIdentifier,
                RequestId: message is RentalRequestMessage rentalRequestMessage ? rentalRequestMessage.RentalRequestId : defaultMissingIdentifier);
            return toReturn;
        }

        public ConversationDTO ConversationToConversationDTO(Conversation conversation)
        {
            var messageDTOs = conversation.Messages
                .OrderBy(messageItem => messageItem.MessageSentTime)
                .Select(messageItem => this.MessageToMessageDTO(messageItem))
                .ToList();

            var participantsOrdered = conversation.Participants
                .OrderBy(participantItem => participantItem.UserId)
                .ToList();

            var lastRead = conversation.Participants.ToDictionary(
                participantItem => participantItem.UserId,
                participantItem => participantItem.LastMessageReadTime ?? DateTime.MinValue);

            return new ConversationDTO(
                conversationId: conversation.ConversationId,
                participants: participantsOrdered,
                messages: messageDTOs,
                lastRead: lastRead);
        }

    }
}

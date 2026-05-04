// <copyright file="ConversationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Enum;
using BookingBoardGames.Src.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Src.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository conversationRepository;
        private readonly IUserRepository userRepository;
        private readonly AppDbContext context;
        private readonly Dictionary<int, IConversationService> subscribers = new Dictionary<int, IConversationService>();

        public ConversationService(IConversationRepository conversationRepo, int userIdInput, AppDbContext appContext)
                    : this(conversationRepo, userIdInput, appContext, App.UserRepository)
        {
        }

        public ConversationService(IConversationRepository conversationRepo, int userIdInput, AppDbContext appContext, IUserRepository userRepo)
        {
            this.UserId = userIdInput;
            this.conversationRepository = conversationRepo;
            this.userRepository = userRepo;
            this.context = appContext;

            this.Subscribe(this.UserId, this);
        }

        private int UserId { get; set; }

        public event Action<MessageDataTransferObject, string> ActionMessageProcessed;

        public event Action<ConversationDTO, string> ActionConversationProcessed;

        public event Action<ReadReceiptDTO> ActionReadReceiptProcessed;

        public event Action<MessageDataTransferObject, string> ActionMessageUpdateProcessed;

        public List<ConversationDTO> FetchConversations()
        {
            return this.conversationRepository
                .GetConversationsForUser(this.UserId)
                .Select(conversation => this.ConversationToConversationDTO(conversation))
                .ToList();
        }

        public string GetOtherUserNameByConversationDTO(ConversationDTO conversation)
        {
            int otherUserId = conversation.Participants
                .First(participants => participants.UserId != this.UserId).UserId;
            return this.userRepository.GetById(otherUserId)?.Username ?? "Unknown User";
        }

        public string GetOtherUserNameByMessageDTO(MessageDataTransferObject message)
        {
            int otherId = message.SenderId == this.UserId ? message.ReceiverId : message.SenderId;
            return this.userRepository.GetById(otherId)?.Username ?? "Unknown User";
        }

        public void SendMessage(MessageDataTransferObject message)
        {
            var entity = this.MessageDTOToMessage(message);
            this.conversationRepository.HandleNewMessage(entity);

            var persisted = this.FetchPersistedMessage(entity.MessageId);
            this.NotifySubscribersAboutMessage(persisted);
        }

        public void UpdateMessage(MessageDataTransferObject message)
        {
            var entity = this.MessageDTOToMessage(message);
            this.conversationRepository.HandleMessageUpdate(entity);

            var persisted = this.FetchPersistedMessage(entity.MessageId);
            this.NotifySubscribersAboutMessageUpdate(persisted);
        }

        public void SendReadReceipt(ConversationDTO conversation)
        {
            var receipt = new ReadReceiptDTO(
                conversation.Id,
                this.UserId,
                conversation.Participants.First(participants => participants.UserId != this.UserId).UserId,
                DateTime.Now);

            this.conversationRepository.HandleReadReceipt(receipt);
            this.NotifySubscribersAboutReadReceipt(receipt);
        }

        public void OnCardPaymentSelected(int messageId)
        {
            this.FinalizeRentalRequest(messageId);
        }

        public void OnCashPaymentSelected(int messageId, int paymentId)
        {
            this.FinalizeRentalRequest(messageId);
            this.CreateCashAgreementMessage(messageId, paymentId);
        }

        public void OnMessageReceived(Message message)
        {
            var dto = this.MessageToMessageDTO(message);
            this.ActionMessageProcessed?.Invoke(dto, this.GetOtherUserNameByMessageDTO(dto));
        }

        public void OnConversationReceived(Conversation conversation)
        {
            var dto = this.ConversationToConversationDTO(conversation);
            this.ActionConversationProcessed?.Invoke(dto, this.GetOtherUserNameByConversationDTO(dto));
        }

        public void OnReadReceiptReceived(ReadReceiptDTO readReceipt)
        {
            this.ActionReadReceiptProcessed?.Invoke(readReceipt);
        }

        public void OnMessageUpdateReceived(Message message)
        {
            var dto = this.MessageToMessageDTO(message);
            this.ActionMessageUpdateProcessed?.Invoke(dto, this.GetOtherUserNameByMessageDTO(dto));
        }

        public void Subscribe(int userId, IConversationService observer)
        {
            this.subscribers[userId] = observer;
        }

        public void Unsubscribe(int userId)
        {
            this.subscribers.Remove(userId);
        }

        public void NotifySubscribersAboutMessage(Message message)
        {
            foreach (int userId in this.GetParticipantUserIds(message.ConversationId))
            {
                if (this.subscribers.TryGetValue(userId, out var observer))
                {
                    observer.OnMessageReceived(message);
                }
            }
        }

        public void NotifySubscribersAboutMessageUpdate(Message message)
        {
            foreach (int userId in this.GetParticipantUserIds(message.ConversationId))
            {
                if (this.subscribers.TryGetValue(userId, out var observer))
                {
                    observer.OnMessageUpdateReceived(message);
                }
            }
        }

        public void NotifySubscribersAboutNewConversation(Conversation conversation)
        {
            foreach (int userId in conversation.Participants.Select(participants => participants.UserId))
            {
                if (this.subscribers.TryGetValue(userId, out var observer))
                {
                    observer.OnConversationReceived(conversation);
                }
            }
        }

        public void NotifySubscribersAboutReadReceipt(ReadReceiptDTO readReceipt)
        {
            foreach (int userId in this.GetParticipantUserIds(readReceipt.ConversationId))
            {
                if (this.subscribers.TryGetValue(userId, out var observer))
                {
                    observer.OnReadReceiptReceived(readReceipt);
                }
            }
        }

        public Message MessageDTOToMessage(MessageDataTransferObject messageDto)
        {
            return messageDto.Type switch
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
        }

        public MessageDataTransferObject MessageToMessageDTO(Message message)
        {
            const int defaultMissingIdentifier = -1;

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
                RentalRequestMessage rentalReq => rentalReq.RequestContent ?? rentalReq.MessageContentAsString ?? string.Empty,
                SystemMessage systemMessage => systemMessage.MessageContent ?? systemMessage.MessageContentAsString ?? string.Empty,
                _ => message.MessageContentAsString ?? string.Empty,
            };

            return new MessageDataTransferObject(
                Id: message.MessageId,
                ConversationId: message.ConversationId,
                SenderId: message.MessageSenderId,
                ReceiverId: message.MessageReceiverId,
                SentAt: message.MessageSentTime,
                Content: content,
                Type: messageType,
                ImageUrl: message is ImageMessage imageMessage ? imageMessage.MessageImageUrl ?? string.Empty : string.Empty,
                IsResolved: message is RentalRequestMessage rentalRequest ? rentalRequest.IsRequestResolved
                          : message is CashAgreementMessage cashAgreement && cashAgreement.IsCashAgreementResolved,
                IsAccepted: message is RentalRequestMessage rentalRequestMessage ? rentalRequestMessage.IsRequestAccepted : false,
                IsAcceptedByBuyer: message is CashAgreementMessage cashAgreementBuyer ? cashAgreementBuyer.IsCashAgreementAcceptedByBuyer : false,
                IsAcceptedBySeller: message is CashAgreementMessage cashAgreementSeller ? cashAgreementSeller.IsCashAgreementAcceptedBySeller : false,
                PaymentId: message is CashAgreementMessage cashPayment ? cashPayment.CashPaymentId : defaultMissingIdentifier,
                RequestId: message is RentalRequestMessage request ? request.RentalRequestId : defaultMissingIdentifier);
        }

        public ConversationDTO ConversationToConversationDTO(Conversation conversation)
        {
            var messageDTOs = conversation.Messages
                .OrderBy(message => message.MessageSentTime)
                .Select(message => this.MessageToMessageDTO(message))
                .ToList();

            var participantsOrdered = conversation.Participants
                .OrderBy(participants => participants.UserId)
                .ToList();

            var lastRead = conversation.Participants.ToDictionary(
                participants => participants.UserId,
                participants => participants.LastMessageReadTime ?? DateTime.MinValue);

            return new ConversationDTO(
                conversationId: conversation.ConversationId,
                participants: participantsOrdered,
                messages: messageDTOs,
                lastRead: lastRead);
        }

        private void FinalizeRentalRequest(int messageId)
        {
            this.conversationRepository.HandleRentalRequestFinalization(messageId);

            var persisted = this.FetchPersistedMessage(messageId);
            this.NotifySubscribersAboutMessageUpdate(persisted);
        }

        private void CreateCashAgreementMessage(int parentMessageId, int paymentId)
        {
            this.conversationRepository.CreateCashAgreementMessage(parentMessageId, paymentId);

            var persisted = this.context.Messages
                .Include(message => message.Sender)
                .Include(message => message.Receiver)
                .Include(message => message.Conversation)
                .OfType<CashAgreementMessage>()
                .OrderByDescending(message => message.MessageId)
                .First();

            this.NotifySubscribersAboutMessage(persisted);
        }

        private Message FetchPersistedMessage(int messageId)
        {
            return this.context.Messages
                .Include(message => message.Sender)
                .Include(message => message.Receiver)
                .Include(message => message.Conversation)
                .First(message => message.MessageId == messageId);
        }

        private List<int> GetParticipantUserIds(int conversationId)
        {
            return this.context.ConversationParticipants
                .Where(participants => participants.ConversationId == conversationId)
                .Select(participants => participants.UserId)
                .ToList();
        }
    }
}

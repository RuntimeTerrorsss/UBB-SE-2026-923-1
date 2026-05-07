// <copyright file="ConversationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.DTO;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Data.Interfaces;

namespace BookingBoardGames.Src.Services
{
    public class ConversationService : IConversationService
    {
        private IConversationRepository ConversationRepository { get; set; }

        private IUserRepository userRepository;
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
            this.ConversationRepository = conversationRepo;
            this.userRepository = userRepo;
            this.notifier = conversationNotifier;

            this.notifier.Register(this.UserId, this);
        }

        private static IConversationNotifier ResolveNotifier()
        {
            return App.ConversationNotifier ?? new ConversationNotifier();
        }

        private async Task NotifySubscribersAboutMessage(Message message)
        {
            IReadOnlyList<int> participants = await this.ConversationRepository.GetParticipantUserIds(message.ConversationId);
            this.notifier.NotifyMessage(participants, message);
        }

        private async Task NotifySubscribersAboutMessageUpdate(Message message)
        {
            IReadOnlyList<int> participants = await this.ConversationRepository.GetParticipantUserIds(message.ConversationId);
            this.notifier.NotifyMessageUpdate(participants, message);
        }

        private async Task NotifySubscribersAboutReadReceipt(ReadReceiptDTO readReceipt)
        {
            IReadOnlyList<int> participants = await this.ConversationRepository.GetParticipantUserIds(readReceipt.ConversationId);
            this.notifier.NotifyReadReceipt(participants, readReceipt);
        }

        private void NotifySubscribersAboutNewConversation(Conversation conversation)
        {
            this.notifier.NotifyNewConversation(conversation);
        }

        public async Task<List<ConversationDTO>> FetchConversations()
        {
            List<ConversationDTO> conversationList = new List<ConversationDTO>();

            foreach (var conversation in await this.ConversationRepository.GetConversationsForUser(this.UserId))
            {
                conversationList.Add(this.ConversationToConversationDTO(conversation));
            }

            return conversationList;
        }

        public string GetOtherUserNameByConversationDTO(ConversationDTO conversation)
        {
            int otherUserId = conversation.Participants.First(participantItem => participantItem.UserId != this.UserId).UserId;
            var user = this.userRepository.GetById(otherUserId).GetAwaiter().GetResult();
            return user?.Username ?? "Unknown User";
        }

        public string GetOtherUserNameByMessageDTO(MessageDataTransferObject message)
        {
            var user = this.userRepository.GetById(message.SenderId == this.UserId ? message.ReceiverId : message.SenderId).GetAwaiter().GetResult();
            return user?.Username ?? "Unknown User";
        }

        public async Task SendMessage(MessageDataTransferObject message)
        {
            Message persisted = await this.ConversationRepository.HandleNewMessage(this.MessageDTOToMessage(message));
            await this.NotifySubscribersAboutMessage(persisted);
        }

        public async Task<int> CreateConversation(int senderId, int receiverId)
        {
            int conversationId = await this.ConversationRepository.CreateConversation(senderId, receiverId);
            Conversation createdConversation = await this.ConversationRepository.GetConversationById(conversationId);
            this.NotifySubscribersAboutNewConversation(createdConversation);
            return conversationId;
        }

        public async Task UpdateMessage(MessageDataTransferObject message)
        {
            Message? persisted = await this.ConversationRepository.HandleMessageUpdate(this.MessageDTOToMessage(message));
            if (persisted != null)
            {
                await this.NotifySubscribersAboutMessageUpdate(persisted);
            }
        }

        public async Task SendReadReceipt(ConversationDTO conversation)
        {
            var readReceipt = new ReadReceiptDTO(
                conversation.Id,
                this.UserId,
                conversation.Participants.First(participantItem => participantItem.UserId != this.UserId).UserId,
                DateTime.Now);
            await this.ConversationRepository.HandleReadReceipt(readReceipt);
            await this.NotifySubscribersAboutReadReceipt(readReceipt);
        }

        public async Task OnCardPaymentSelected(int messageId)
        {
            await this.FinalizeRentalRequest(messageId);
        }

        public async Task OnCashPaymentSelected(int messageId, int paymentId)
        {
            await this.FinalizeRentalRequest(messageId);
            await this.SendCashAgreementMessage(messageId, paymentId);
        }

        private async Task FinalizeRentalRequest(int messageId)
        {
            Message? updated = await this.ConversationRepository.HandleRentalRequestFinalization(messageId);
            if (updated != null)
            {
                await this.NotifySubscribersAboutMessageUpdate(updated);
            }
        }

        private async Task SendCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            Message? created = await this.ConversationRepository.CreateCashAgreementMessage(messageIdOfParentRentalRequestMessage, paymentId);
            if (created != null)
            {
                await this.NotifySubscribersAboutMessage(created);
            }
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

            return new MessageDataTransferObject(
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

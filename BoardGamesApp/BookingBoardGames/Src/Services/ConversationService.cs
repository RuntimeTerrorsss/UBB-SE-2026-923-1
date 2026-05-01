// <copyright file="ConversationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Enum;
using BookingBoardGames.Src.Repositories;


namespace BookingBoardGames.Src.Services
{
    public class ConversationService : IConversationService
    {
        private IConversationRepository ConversationRepository { get; set; }

        private IUserRepository userRepository;

        private int UserId { get; set; }

        public event Action<MessageDataTransferObject, string> ActionMessageProcessed;

        public event Action<ConversationDTO, string> ActionConversationProcessed;

        public event Action<ReadReceiptDTO> ActionReadReceiptProcessed;

        public event Action<MessageDataTransferObject, string> ActionMessageUpdateProcessed;

        public ConversationService(IConversationRepository conversationRepo, int userIdInput)
            : this(conversationRepo, userIdInput, App.UserRepository)
        {
        }

        public ConversationService(IConversationRepository conversationRepo, int userIdInput, IUserRepository userRepo)
        {
            this.UserId = userIdInput;
            this.ConversationRepository = conversationRepo;
            this.userRepository = userRepo;

            this.ConversationRepository.Subscribe(UserId, this);
        }

        public List<ConversationDTO> FetchConversations()
        {
            List<ConversationDTO> conversationList = new List<ConversationDTO>();

            foreach (var conversation in this.ConversationRepository.GetConversationsForUser(UserId))
            {
                conversationList.Add(ConversationToConversationDTO(conversation));
            }

            return conversationList;
        }

        public string GetOtherUserNameByConversationDTO(ConversationDTO conversation)
        {
            int firstParticipantIndex = 0;
            int secondParticipantIndex = 1;
            var user = this.userRepository.GetById(conversation.Participants[firstParticipantIndex] == UserId ? conversation.Participants[secondParticipantIndex] : conversation.Participants[firstParticipantIndex]);
            return user?.Username ?? "Unknown User";
        }

        public string GetOtherUserNameByMessageDTO(MessageDataTransferObject message)
        {
            return userRepository.GetById(message.SenderId == UserId ? message.ReceiverId : message.SenderId).Username ?? "Unknown User";
        }

        public void SendMessage(MessageDataTransferObject message)
        {
            this.ConversationRepository.HandleNewMessage(MessageDTOToMessage(message));
        }

        public void UpdateMessage(MessageDataTransferObject message)
        {
            this.ConversationRepository.HandleMessageUpdate(MessageDTOToMessage(message));
        }

        public void SendReadReceipt(ConversationDTO conversation)
        {
            this.ConversationRepository.HandleReadReceipt(new ReadReceiptDTO(
                conversation.Id,
                UserId,
                conversation.Participants.First(participant => participant != UserId),
                DateTime.Now));
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
            this.ConversationRepository.HandleRentalRequestFinalization(messageId);
        }

        private void SendCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            this.ConversationRepository.CreateCashAgreementMessage(messageIdOfParentRentalRequestMessage, paymentId);
        }

        public void OnMessageReceived(Message message)
        {
            MessageDataTransferObject messageDTO = MessageToMessageDTO(message);
            string userName = GetOtherUserNameByMessageDTO(messageDTO);
            ActionMessageProcessed?.Invoke(messageDTO, userName);
        }

        public void OnConversationReceived(Conversation conversation)
        {
            ConversationDTO conversationDTO = ConversationToConversationDTO(conversation);
            string userName = GetOtherUserNameByConversationDTO(conversationDTO);
            ActionConversationProcessed?.Invoke(conversationDTO, userName);
        }

        public void OnReadReceiptReceived(ReadReceiptDTO readReceipt)
        {
            ActionReadReceiptProcessed?.Invoke(readReceipt);
        }

        public void OnMessageUpdateReceived(Message message)
        {
            MessageDataTransferObject messageDTO = MessageToMessageDTO(message);
            string userName = GetOtherUserNameByMessageDTO(messageDTO);
            ActionMessageUpdateProcessed?.Invoke(messageDTO, userName);
        }

        public Message MessageDTOToMessage(MessageDataTransferObject messageDto)
        {
            Message toReturn = messageDto.Type switch
            {
                MessageType.MessageText => new TextMessage(
                    MessageId: messageDto.Id,
                    conversationId: messageDto.ConversationId,
                    senderId: messageDto.SenderId,
                    receiverId: messageDto.ReceiverId,
                    sentAt: messageDto.SentAt,
                    content: messageDto.Content),
                MessageType.MessageImage => new ImageMessage(
                    id: messageDto.Id,
                    conversationId: messageDto.ConversationId,
                    senderId: messageDto.SenderId,
                    receiverId: messageDto.ReceiverId,
                    sentAt: messageDto.SentAt,
                    imageUrl: messageDto.ImageUrl),
                MessageType.MessageRentalRequest => new RentalRequestMessage(
                    id: messageDto.Id,
                    conversationId: messageDto.ConversationId,
                    senderId: messageDto.SenderId,
                    receiverId: messageDto.ReceiverId,
                    sentAt: messageDto.SentAt,
                    content: messageDto.Content,
                    requestId: messageDto.RequestId,
                    isResolved: messageDto.IsResolved,
                    isAccepted: messageDto.IsAccepted),
                MessageType.MessageCashAgreement => new CashAgreementMessage(
                    id: messageDto.Id,
                    conversationId: messageDto.ConversationId,
                    sellerId: messageDto.SenderId,
                    buyerId: messageDto.ReceiverId,
                    paymentId: messageDto.PaymentId,
                    sentAt: messageDto.SentAt,
                    content: messageDto.Content,
                    isResolved: messageDto.IsResolved,
                    isAcceptedByBuyer: messageDto.IsAcceptedByBuyer,
                    isAcceptedBySeller: messageDto.IsAcceptedBySeller),
                MessageType.MessageSystem => new SystemMessage(
                    id: messageDto.Id,
                    conversationId: messageDto.ConversationId,
                    sentAt: messageDto.SentAt,
                    content: messageDto.Content),
            };
            return toReturn;
        }

        public MessageDataTransferObject MessageToMessageDTO(Message message)
        {
            int defaultMissingIdentifier = -1;

            MessageDataTransferObject toReturn = new MessageDataTransferObject(
                Id: message.MessageId,
                ConversationId: message.ConversationId,
                SenderId: message.MessageSenderId,
                ReceiverId: message.MessageReceiverId,
                SentAt: message.MessageSentTime,
                Content: message.MessageContentAsString,
                Type: message.TypeOfMessage,
                ImageUrl: message is ImageMessage imageMessage ? imageMessage.MessageImageUrl : string.Empty,
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
            var messageDTOs = conversation.ConversationMessageList.Select(messageItem => MessageToMessageDTO(messageItem)).ToList();
            return new ConversationDTO(
                conversationId: conversation.ConversationId,
                participants: conversation.ConversationParticipantIds,
                messages: messageDTOs,
                lastRead: conversation.LastMessageReadTime);
        }

    }
}

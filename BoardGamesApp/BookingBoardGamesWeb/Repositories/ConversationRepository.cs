using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Enum;
using BookingBoardGames.Data.DTO;
using Microsoft.EntityFrameworkCore;
using BookingBoardGames.Data.Interfaces;

namespace BookingBoardGames.Web.Interfaces
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly AppDbContext context;

        public ConversationRepository(AppDbContext appContext)
        {
            this.context = appContext;
        }

        public List<Conversation> GetConversationsForUser(int userId)
        {
            return this.context.Conversations
                .AsNoTracking()
                .Include(conversation => conversation.Participants)
                .Include(conversation => conversation.Messages)
                .Where(conversation => conversation.Participants.Any(participant => participant.UserId == userId))
                .ToList();
        }

        public async Task<Conversation> GetConversationById(int conversationId)
        {
            var found = this.context.Conversations
                .Include(conversation => conversation.Participants)
                .Include(conversation => conversation.Messages)
                .FirstOrDefault(conversation => conversation.ConversationId == conversationId);

            if (found is null)
            {
                throw new InvalidOperationException($"Conversation {conversationId} was not found.");
            }

            return found;
        }

        public IReadOnlyList<int> GetParticipantUserIds(int conversationId)
        {
            return this.context.ConversationParticipants
                .Where(participant => participant.ConversationId == conversationId)
                .Select(participant => participant.UserId)
                .ToList();
        }

        public async Task<int> CreateConversation(int senderId, int receiverId)
        {
            var conversation = new Conversation
            {
                Participants = new List<ConversationParticipant>
                {
                    new ConversationParticipant { UserId = senderId },
                    new ConversationParticipant { UserId = receiverId },
                },
                Messages = new List<Message>(),
            };

            this.context.Conversations.Add(conversation);
            this.context.SaveChanges();

            var conversation = response.Content.ReadFromJsonAsync<Conversation>().GetAwaiter().GetResult();
            return conversation?.ConversationId ?? -1;
        }

        public async Task<Message> HandleNewMessage(Message message)
        {
            message.MessageId = 0;
            this.context.Messages.Add(message);
            this.context.SaveChanges();

            return this.context.Messages
                .Include(newMessage => newMessage.Sender)
                .Include(newMessage => newMessage.Receiver)
                .Include(newMessage => newMessage.Conversation)
                .First(newMessage => newMessage.MessageId == message.MessageId);
        }

        public async Task<Message?> HandleMessageUpdate(Message message)
        {
            var tracked = this.context.Messages
                .FirstOrDefault(m => m.MessageId == message.MessageId);

            if (response.IsSuccessStatusCode)
            {
                var persistedDto = response.Content.ReadFromJsonAsync<MessageDataTransferObject>().GetAwaiter().GetResult();
                if (persistedDto != null) return MapDtoToEntity(persistedDto);
            }
            return null;
        }

        public void HandleReadReceipt(ReadReceiptDTO readReceipt)
        {
            App.Client.PostAsJsonAsync("conversation/readreceipt", readReceipt).GetAwaiter().GetResult();
        }

            this.context.SaveChanges();

            return this.context.Messages
                .Include(newMessage => newMessage.Sender)
                .Include(newMessage => newMessage.Receiver)
                .Include(newMessage => newMessage.Conversation)
                .First(newMessage => newMessage.MessageId == message.MessageId);
        }

        public void HandleReadReceipt(ReadReceiptDTO readReceipt)
        {
            var participant = this.context.ConversationParticipants
                .FirstOrDefault(
                    receiverParticipant => receiverParticipant.ConversationId == readReceipt.ConversationId
                         && receiverParticipant.UserId == readReceipt.ReaderId);

            if (participant is null)
            {
                var dto = response.Content.ReadFromJsonAsync<MessageDataTransferObject>().GetAwaiter().GetResult();
                if (dto != null) return MapDtoToEntity(dto);
            }

            participant.LastMessageReadTime = readReceipt.ReceiptTimeStamp;
            this.context.SaveChanges();
        }

        public Message? HandleRentalRequestFinalization(int messageId)
        {
            var rentalMessage = this.context.Messages
                .OfType<RentalRequestMessage>()
                .FirstOrDefault(requestMessage => requestMessage.MessageId == messageId);

            if (rentalMessage is null)
            {
                var dto = response.Content.ReadFromJsonAsync<MessageDataTransferObject>().GetAwaiter().GetResult();
                if (dto != null) return MapDtoToEntity(dto);
            }
            return null;
        }

            rentalMessage.IsRequestResolved = true;
            rentalMessage.IsRequestAccepted = true;
            this.context.SaveChanges();

            return this.context.Messages
                .Include(requestMessage => requestMessage.Sender)
                .Include(requestMessage => requestMessage.Receiver)
                .Include(requestMessage => requestMessage.Conversation)
                .First(requestMessage => requestMessage.MessageId == messageId);
        }

        public Message? CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            var parent = this.context.Messages
                .OfType<RentalRequestMessage>()
                .FirstOrDefault(m => m.MessageId == messageIdOfParentRentalRequestMessage);

            if (parent is null)
            {
                return null;
            }

            var cashMessage = new CashAgreementMessage
            {
                MessageType.MessageText => new TextMessage { TextMessageContent = dto.Content, Conversation = null!, Sender = null!, Receiver = null! },
                MessageType.MessageImage => new ImageMessage { MessageImageUrl = dto.ImageUrl, Conversation = null!, Sender = null!, Receiver = null! },
                MessageType.MessageRentalRequest => new RentalRequestMessage { RentalRequestId = dto.RequestId, IsRequestResolved = dto.IsResolved, IsRequestAccepted = dto.IsAccepted, RequestContent = dto.Content, Conversation = null!, Sender = null!, Receiver = null! },
                MessageType.MessageCashAgreement => new CashAgreementMessage { CashPaymentId = dto.PaymentId, IsCashAgreementResolved = dto.IsResolved, IsCashAgreementAcceptedByBuyer = dto.IsAcceptedByBuyer, IsCashAgreementAcceptedBySeller = dto.IsAcceptedBySeller, Conversation = null!, Sender = null!, Receiver = null! },
                MessageType.MessageSystem => new SystemMessage { MessageContent = dto.Content, Conversation = null!, Sender = null!, Receiver = null! },
                _ => throw new ArgumentOutOfRangeException()
            };

            this.context.Messages.Add(cashMessage);
            this.context.SaveChanges();

            return this.context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Conversation)
                .First(m => m.MessageId == cashMessage.MessageId);
        }
    }
}

// <copyright file="ConversationRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Src.DTO;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Src.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly AppDbContext context;

        public ConversationRepository(AppDbContext appContext)
        {
            this.context = appContext;
        }

        public async Task<List<Conversation>> GetConversationsForUser(int userId)
        {
            return await this.context.Conversations
                .AsNoTracking()
                .Include(conversation => conversation.Participants)
                .Include(conversation => conversation.Messages)
                .Where(conversation => conversation.Participants.Any(participant => participant.UserId == userId))
                .ToListAsync();
        }

        public async Task<Conversation> GetConversationById(int conversationId)
        {
            var found = await this.context.Conversations
                .Include(conversation => conversation.Participants)
                .Include(conversation => conversation.Messages)
                .FirstOrDefaultAsync(conversation => conversation.ConversationId == conversationId);

            if (found is null)
            {
                throw new InvalidOperationException($"Conversation {conversationId} was not found.");
            }

            return found;
        }

        public async Task<IReadOnlyList<int>> GetParticipantUserIds(int conversationId)
        {
            return await this.context.ConversationParticipants
                .Where(participant => participant.ConversationId == conversationId)
                .Select(participant => participant.UserId)
                .ToListAsync();
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
            await this.context.SaveChangesAsync();

            return conversation.ConversationId;
        }

        public async Task<Message> HandleNewMessage(Message message)
        {
            message.MessageId = 0;
            this.context.Messages.Add(message);
            await this.context.SaveChangesAsync();

            return await this.context.Messages
                .Include(newMessage => newMessage.Sender)
                .Include(newMessage => newMessage.Receiver)
                .Include(newMessage => newMessage.Conversation)
                .FirstAsync(newMessage => newMessage.MessageId == message.MessageId);
        }

        public async Task<Message?> HandleMessageUpdate(Message message)
        {
            var tracked = await this.context.Messages
                .FirstOrDefaultAsync(newmessage => newmessage.MessageId == message.MessageId);

            if (tracked is null)
            {
                return null;
            }

            this.context.Entry(tracked).CurrentValues.SetValues(message);

            if (tracked is RentalRequestMessage rentalTracked && message is RentalRequestMessage rentalIncoming)
            {
                rentalTracked.IsRequestResolved = rentalIncoming.IsRequestResolved;
                rentalTracked.IsRequestAccepted = rentalIncoming.IsRequestAccepted;
                rentalTracked.RequestContent = rentalIncoming.RequestContent;
            }
            else if (tracked is CashAgreementMessage cashTracked && message is CashAgreementMessage cashIncoming)
            {
                cashTracked.IsCashAgreementResolved = cashIncoming.IsCashAgreementResolved;
                cashTracked.IsCashAgreementAcceptedByBuyer = cashIncoming.IsCashAgreementAcceptedByBuyer;
                cashTracked.IsCashAgreementAcceptedBySeller = cashIncoming.IsCashAgreementAcceptedBySeller;
            }

            await this.context.SaveChangesAsync();

            return await this.context.Messages
                .Include(newMessage => newMessage.Sender)
                .Include(newMessage => newMessage.Receiver)
                .Include(newMessage => newMessage.Conversation)
                .FirstAsync(newMessage => newMessage.MessageId == message.MessageId);
        }

        public async Task HandleReadReceipt(ReadReceiptDTO readReceipt)
        {
            var participant = await this.context.ConversationParticipants
                .FirstOrDefaultAsync(
                    receiverParticipant => receiverParticipant.ConversationId == readReceipt.ConversationId
                         && receiverParticipant.UserId == readReceipt.ReaderId);

            if (participant is null)
            {
                return;
            }

            participant.LastMessageReadTime = readReceipt.ReceiptTimeStamp;
            await this.context.SaveChangesAsync();
        }

        public async Task<Message?> HandleRentalRequestFinalization(int messageId)
        {
            var rentalMessage = await this.context.Messages
                .OfType<RentalRequestMessage>()
                .FirstOrDefaultAsync(requestMessage => requestMessage.MessageId == messageId);

            if (rentalMessage is null)
            {
                return null;
            }

            rentalMessage.IsRequestResolved = true;
            rentalMessage.IsRequestAccepted = true;
            await this.context.SaveChangesAsync();

            return await this.context.Messages
                .Include(requestMessage => requestMessage.Sender)
                .Include(requestMessage => requestMessage.Receiver)
                .Include(requestMessage => requestMessage.Conversation)
                .FirstAsync(requestMessage => requestMessage.MessageId == messageId);
        }

        public async Task<Message?> CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            var parent = await this.context.Messages
                .OfType<RentalRequestMessage>()
                .FirstOrDefaultAsync(paymentMessage => paymentMessage.MessageId == messageIdOfParentRentalRequestMessage);

            if (parent is null)
            {
                return null;
            }

            var cashMessage = new CashAgreementMessage
            {
                ConversationId = parent.ConversationId,
                MessageSenderId = parent.MessageSenderId,
                MessageReceiverId = parent.MessageReceiverId,
                CashPaymentId = paymentId,
                MessageSentTime = DateTime.UtcNow,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            this.context.Messages.Add(cashMessage);
            await this.context.SaveChangesAsync();

            return await this.context.Messages
                .Include(message => message.Sender)
                .Include(message => message.Receiver)
                .Include(message => message.Conversation)
                .FirstAsync(message => message.MessageId == cashMessage.MessageId);
        }
    }
}

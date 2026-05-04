// <copyright file="ConversationRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Services;
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

        public List<Conversation> GetConversationsForUser(int userId)
        {
            return this.context.Conversations
                .AsNoTracking()
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .ToList();
        }

        public Conversation GetConversationById(int conversationId)
        {
            var found = this.context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .FirstOrDefault(c => c.ConversationId == conversationId);

            if (found is null)
            {
                throw new InvalidOperationException($"Conversation {conversationId} was not found.");
            }

            return found;
        }

        public int CreateConversation(int senderId, int receiverId)
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

            return conversation.ConversationId;
        }

        public void HandleNewMessage(Message message)
        {
            message.MessageId = 0;
            this.context.Messages.Add(message);
            this.context.SaveChanges();
        }

        public void HandleMessageUpdate(Message message)
        {
            var tracked = this.context.Messages
                .FirstOrDefault(m => m.MessageId == message.MessageId);

            if (tracked is null)
            {
                return;
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

            this.context.SaveChanges();
        }

        public void HandleReadReceipt(ReadReceiptDTO readReceipt)
        {
            var participant = this.context.ConversationParticipants
                .FirstOrDefault(p =>
                    p.ConversationId == readReceipt.ConversationId &&
                    p.UserId == readReceipt.ReaderId);

            if (participant is null)
            {
                return;
            }

            participant.LastMessageReadTime = readReceipt.ReceiptTimeStamp;
            this.context.SaveChanges();
        }

        public void HandleRentalRequestFinalization(int messageId)
        {
            var rentalMessage = this.context.Messages
                .OfType<RentalRequestMessage>()
                .FirstOrDefault(m => m.MessageId == messageId);

            if (rentalMessage is null)
            {
                return;
            }

            rentalMessage.IsRequestResolved = true;
            rentalMessage.IsRequestAccepted = true;
            this.context.SaveChanges();
        }

        public void CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            var parent = this.context.Messages
                .OfType<RentalRequestMessage>()
                .FirstOrDefault(m => m.MessageId == messageIdOfParentRentalRequestMessage);

            if (parent is null)
            {
                return;
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
            this.context.SaveChanges();
        }

        public List<int> GetParticipantUserIds(int conversationId)
        {
            return this.context.ConversationParticipants
                .Where(p => p.ConversationId == conversationId)
                .Select(p => p.UserId)
                .ToList();
        }
    }
}

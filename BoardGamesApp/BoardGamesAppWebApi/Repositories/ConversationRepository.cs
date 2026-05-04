// <copyright file="ConversationRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Src.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly AppDbContext context;
        private readonly Dictionary<int, IConversationService> subscribers = new Dictionary<int, IConversationService>();

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

        public Conversation GetConversationById(int conversationId)
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

            var persisted = this.context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .First(c => c.ConversationId == conversation.ConversationId);

            this.NotifySubscribersAboutNewConversation(persisted);

            return conversation.ConversationId;
        }

        public void HandleNewMessage(Message message)
        {
            message.MessageId = 0;
            this.context.Messages.Add(message);
            this.context.SaveChanges();

            var persisted = this.context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Conversation)
                .First(m => m.MessageId == message.MessageId);

            this.NotifySubscribersAboutMessage(persisted);
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

            var persisted = this.context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Conversation)
                .First(m => m.MessageId == message.MessageId);

            this.NotifySubscribersAboutMessageUpdate(persisted);
        }

        public void HandleReadReceipt(ReadReceiptDTO readReceipt)
        {
            var participant = this.context.ConversationParticipants
                .FirstOrDefault(
                    p => p.ConversationId == readReceipt.ConversationId
                         && p.UserId == readReceipt.ReaderId);

            if (participant is null)
            {
                return;
            }

            participant.LastMessageReadTime = readReceipt.ReceiptTimeStamp;
            this.context.SaveChanges();

            this.NotifySubscribersAboutReadReceipt(readReceipt);
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

            var persisted = this.context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Conversation)
                .First(m => m.MessageId == messageId);

            this.NotifySubscribersAboutMessageUpdate(persisted);
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

            var persisted = this.context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Conversation)
                .First(m => m.MessageId == cashMessage.MessageId);

            this.NotifySubscribersAboutMessage(persisted);
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
                if (this.subscribers.TryGetValue(userId, out IConversationService? observer))
                {
                    observer.OnMessageReceived(message);
                }
            }
        }

        public void NotifySubscribersAboutMessageUpdate(Message message)
        {
            foreach (int userId in this.GetParticipantUserIds(message.ConversationId))
            {
                if (this.subscribers.TryGetValue(userId, out IConversationService? observer))
                {
                    observer.OnMessageUpdateReceived(message);
                }
            }
        }

        public void NotifySubscribersAboutNewConversation(Conversation conversation)
        {
            foreach (int userId in conversation.Participants.Select(p => p.UserId))
            {
                if (this.subscribers.TryGetValue(userId, out IConversationService? observer))
                {
                    observer.OnConversationReceived(conversation);
                }
            }
        }

        public void NotifySubscribersAboutReadReceipt(ReadReceiptDTO readReceipt)
        {
            foreach (int userId in this.GetParticipantUserIds(readReceipt.ConversationId))
            {
                if (this.subscribers.TryGetValue(userId, out IConversationService? observer))
                {
                    observer.OnReadReceiptReceived(readReceipt);
                }
            }
        }

        private List<int> GetParticipantUserIds(int conversationId)
        {
            return this.context.ConversationParticipants
                .Where(participant => participant.ConversationId == conversationId)
                .Select(participant => participant.UserId)
                .ToList();
        }
    }
}

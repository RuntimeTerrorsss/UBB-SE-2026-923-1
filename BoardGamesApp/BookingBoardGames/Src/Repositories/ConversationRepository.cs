// <copyright file="ConversationRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookingBoardGames.Src.Models;
using BookingBoardGames.Src.Services;
using Microsoft.EntityFrameworkCore;
using MessageType = BookingBoardGames.Src.Models;

namespace BookingBoardGames.Src.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly AppDbContextFactory contextFactory = new();

        private Dictionary<int, IConversationService> Subscribers { get; set; } = new();

        public List<Conversation> GetConversationsForUser(int userId)
        {
            using var context = this.contextFactory.CreateDbContext([]);
            return context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .Include(c => c.LastReadByUser)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .ToList();
        }

        public Conversation GetConversationById(int conversationId)
        {
            using var context = this.contextFactory.CreateDbContext([]);
            return context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .Include(c => c.LastReadByUser)
                .FirstOrDefault(c => c.ConversationId == conversationId);
        }

        public void HandleNewMessage(Message message)
        {
            var conversation = this.GetConversationById(message.ConversationId);
            if (conversation is null)
            {
                throw new InvalidOperationException("Conversation not found.");
            }

            using var context = this.contextFactory.CreateDbContext([]);
            context.Messages.Add(message);
            context.SaveChanges();

            message.MessageId = message.MessageId;
            this.NotifySubscribersAboutMessage(message);
        }

        public void HandleReadReceipt(ReadReceipt readReceipt)
        {
            using var context = this.contextFactory.CreateDbContext([]);
            var conversationUser = context.ConversationUsers
                .FirstOrDefault(cu => cu.ConversationId == readReceipt.conversationId
                                   && cu.UserId == readReceipt.messageReaderId);

            if (conversationUser is not null)
            {
                conversationUser.LastRead = readReceipt.timeStamp;
                context.SaveChanges();
            }

            var participantIds = context.ConversationUsers
                .Where(cu => cu.ConversationId == readReceipt.conversationId)
                .Select(cu => cu.UserId)
                .ToList();

            foreach (var participantId in participantIds)
            {
                if (this.Subscribers.TryGetValue(participantId, out var service))
                {
                    service.OnReadReceiptReceived(readReceipt);
                }
            }
        }

        public void HandleMessageUpdate(Message message)
        {
            if (message is CashAgreementMessage cashAgreementMessage)
            {
                if (cashAgreementMessage.IsCashAgreementAcceptedByBuyer &&
                    cashAgreementMessage.IsCashAgreementAcceptedBySeller)
                {
                    this.UpdateCashPaymentFromMessageUpdate(cashAgreementMessage);
                }
            }

            using var context = this.contextFactory.CreateDbContext([]);
            context.Messages.Update(message);
            context.SaveChanges();

            this.NotifySubscribersAboutMessageUpdate(message);
        }

        public int CreateConversation(int senderId, int receiverId)
        {
            using var context = this.contextFactory.CreateDbContext([]);

            var existing = context.Conversations
                .Include(c => c.Participants)
                .FirstOrDefault(c =>
                    c.Participants.Any(p => p.UserId == senderId) &&
                    c.Participants.Any(p => p.UserId == receiverId) &&
                    c.Participants.Count == 2);

            if (existing is not null)
            {
                return existing.ConversationId;
            }

            var newConversation = new Conversation
            {
                Participants = new List<ConversationParticipant>
                {
                    new ConversationParticipant { UserId = senderId, LastRead = DateTime.Now },
                    new ConversationParticipant { UserId = receiverId, LastRead = DateTime.Now },
                },
                Messages = new List<Message>(),
            };

            context.Conversations.Add(newConversation);
            context.SaveChanges();

            this.NotifySubscribersAboutNewConversation(newConversation);

            this.HandleNewMessage(new SystemMessage
            {
                ConversationId = newConversation.ConversationId,
                MessageSentTime = DateTime.Now,
                MessageContent = "New conversation",
                MessageContentAsString = "New conversation",
            });

            this.NotifySubscribersAboutNewConversation(newConversation);

            return newConversation.ConversationId;
        }

        public void HandleRentalRequestFinalization(int messageId)
        {
            try
            {
                using var context = this.contextFactory.CreateDbContext([]);
                var message = context.Messages.OfType<RentalRequestMessage>()
                    .FirstOrDefault(m => m.MessageId == messageId);

                if (message is null)
                {
                    throw new InvalidOperationException("Message not found or not a rental request.");
                }

                message.IsRequestResolved = true;
                message.RequestContent += "\n\nThis request has been finalized!";
                context.SaveChanges();

                this.NotifySubscribersAboutMessageUpdate(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"You tried to finalize a message that wasn't a rental request... how? {ex.Message}");
            }
        }

        public void CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            try
            {
                using var context = this.contextFactory.CreateDbContext([]);
                var parentMessage = context.Messages.OfType<RentalRequestMessage>()
                    .FirstOrDefault(m => m.MessageId == messageIdOfParentRentalRequestMessage);

                if (parentMessage is null)
                {
                    throw new InvalidOperationException("Parent rental request message not found.");
                }

                var cashAgreementMessage = new CashAgreementMessage(
                    -1,
                    parentMessage.ConversationId,
                    parentMessage.MessageReceiverId,
                    parentMessage.MessageSenderId,
                    paymentId,
                    DateTime.Now,
                    $"Cash agreement for request: {parentMessage.RentalRequestId}");

                this.HandleNewMessage(cashAgreementMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"You tried to create a cash agreement message for a message that wasn't a rental request... how? {ex.Message}");
            }
        }

        public void CreateSystemMessageForCashAgreementFinalization(int conversationId, string legalDocumentFilePath)
        {
            var systemMessage = new SystemMessage(
                -1,
                conversationId,
                DateTime.Now,
                $"The cash agreement has been finalized! Here is your receipt: {legalDocumentFilePath}");

            this.HandleNewMessage(systemMessage);
        }

        public void Subscribe(int userId, IConversationService observer)
        {
            if (!this.Subscribers.ContainsKey(userId))
            {
                this.Subscribers.Add(userId, observer);
            }
        }

        public void Unsubscribe(int userId)
        {
            this.Subscribers.Remove(userId);
        }

        public void NotifySubscribersAboutMessage(Message message)
        {
            int[] participants;
            if (message.TypeOfMessage == MessageType.MessageSystem)
            {
                using var context = this.contextFactory.CreateDbContext([]);
                participants = context.ConversationUsers
                    .Where(cu => cu.ConversationId == message.ConversationId)
                    .Select(cu => cu.UserId)
                    .ToArray();
            }
            else
            {
                participants = new[] { message.MessageSenderId, message.MessageReceiverId };
            }

            foreach (var participant in participants)
            {
                if (this.Subscribers.TryGetValue(participant, out var service))
                {
                    service.OnMessageReceived(message);
                }
            }
        }

        public void NotifySubscribersAboutMessageUpdate(Message message)
        {
            var participants = new[] { message.MessageSenderId, message.MessageReceiverId };
            foreach (var participant in participants)
            {
                if (this.Subscribers.TryGetValue(participant, out var service))
                {
                    service.OnMessageUpdateReceived(message);
                }
            }
        }

        public void NotifySubscribersAboutNewConversation(Conversation conversation)
        {
            foreach (var participant in conversation.ConversationParticipantIds)
            {
                if (this.Subscribers.TryGetValue(participant, out var service))
                {
                    service.OnConversationReceived(conversation);
                }
            }
        }

        public void NotifySubscribersAboutReadReceipt(ReadReceipt readReceipt)
        {
            using var context = this.contextFactory.CreateDbContext([]);
            var participantIds = context.ConversationUsers
                .Where(cu => cu.ConversationId == readReceipt.conversationId)
                .Select(cu => cu.UserId)
                .ToList();

            foreach (var participantId in participantIds)
            {
                if (this.Subscribers.TryGetValue(participantId, out var service))
                {
                    service.OnReadReceiptReceived(readReceipt);
                }
            }
        }

        public Conversation? GetGameById(int id)
        {
            throw new NotImplementedException();
        }

        public List<Conversation> GetAll()
        {
            throw new NotImplementedException();
        }

        private void UpdateCashPaymentFromMessageUpdate(CashAgreementMessage message)
        {
            int paymentId = message.CashPaymentId;
            if (!App.CashPaymentService.IsDeliveryConfirmed(paymentId) && message.IsCashAgreementAcceptedByBuyer)
            {
                App.CashPaymentService.ConfirmDelivery(paymentId);
            }

            if (!App.CashPaymentService.IsPaymentConfirmed(paymentId) && message.IsCashAgreementAcceptedBySeller)
            {
                App.CashPaymentService.ConfirmPayment(paymentId);
            }

            if (App.CashPaymentService.IsAllConfirmed(paymentId))
            {
                CreateSystemMessageForCashAgreementFinalization(message.ConversationId, App.CashPaymentService.GetReceipt(paymentId));
            }
        }
    }
}

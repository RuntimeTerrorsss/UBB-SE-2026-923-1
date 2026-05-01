// <copyright file="ConversationRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;
using BookingBoardGames.Src.Services;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Src.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly AppDbContextFactory contextFactory = new();

        public Conversation? GetById(int id)
        {
            using var context = this.contextFactory.CreateDbContext([]);

            return context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .FirstOrDefault(c => c.ConversationId == id);
        }

        public List<Conversation> GetAll()
        {
            using var context = this.contextFactory.CreateDbContext([]);

            return context.Conversations
                .Include(c => c.Participants)
                .ToList();
        }

        public List<Conversation> GetByUserId(int userId)
        {
            using var context = this.contextFactory.CreateDbContext([]);

            return context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .ToList();
        }

        public int CreateConversation(int senderId, int receiverId)
        {
            using var context = this.contextFactory.CreateDbContext([]);

            var conversation = new Conversation
            {
                Participants = new List<ConversationParticipant>
                {
                    new ConversationParticipant { UserId = senderId },
                    new ConversationParticipant { UserId = receiverId },
                },
                Messages = new List<Message>(),
            };

            context.Conversations.Add(conversation);
            context.SaveChanges();

            return conversation.ConversationId;
        }

        public void AddMessage(Message message)
        {
            using var context = this.contextFactory.CreateDbContext([]);

            context.Messages.Add(message);
            context.SaveChanges();
        }

        public void UpdateMessage(Message message)
        {
            using var context = this.contextFactory.CreateDbContext([]);

            context.Messages.Update(message);
            context.SaveChanges();
        }

        public void DeleteConversation(int conversationId)
        {
            using var context = this.contextFactory.CreateDbContext([]);

            var conversation = context.Conversations
                .FirstOrDefault(c => c.ConversationId == conversationId);

            if (conversation is null)
            {
                return;
            }

            context.Conversations.Remove(conversation);
            context.SaveChanges();
        }

        public List<Conversation> GetConversationsForUser(int userId)
        {
            throw new System.NotImplementedException();
        }

        public Conversation GetConversationById(int conversationId)
        {
            throw new System.NotImplementedException();
        }

        public void HandleNewMessage(Message message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleReadReceipt(ReadReceipt readReceipt)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMessageUpdate(Message message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleRentalRequestFinalization(int messageId)
        {
            throw new System.NotImplementedException();
        }

        public void CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            throw new System.NotImplementedException();
        }

        public void CreateSystemMessageForCashAgreementFinalization(int conversationId, string legalDocumentFilePath)
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(int userId, IConversationService observer)
        {
            throw new System.NotImplementedException();
        }

        public void Unsubscribe(int userId)
        {
            throw new System.NotImplementedException();
        }

        public void NotifySubscribersAboutMessage(Message message)
        {
            throw new System.NotImplementedException();
        }

        public void NotifySubscribersAboutMessageUpdate(Message message)
        {
            throw new System.NotImplementedException();
        }

        public void NotifySubscribersAboutNewConversation(Conversation conversation)
        {
            throw new System.NotImplementedException();
        }

        public void NotifySubscribersAboutReadReceipt(ReadReceipt readReceipt)
        {
            throw new System.NotImplementedException();
        }

        public Conversation? GetGameById(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}

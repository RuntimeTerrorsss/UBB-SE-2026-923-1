// <copyright file="ConversationAPIProxy.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Interfaces;

/// <summary>
/// Proxy repository responsible for reading/writing conversation data via HTTP API.
/// </summary>
namespace BookingBoardGames.Src.Repositories
{
    public class ConversationAPIProxy : IConversationRepository
    {
        private readonly HttpClient httpClient;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public ConversationAPIProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<Conversation>> GetConversationsForUser(int userId)
        {
            return await this.httpClient.GetFromJsonAsync<List<Conversation>>(
                       $"conversation/user/{userId}", JsonOptions)
                   ?? new List<Conversation>();
        }

        public async Task<Conversation> GetConversationById(int conversationId)
        {
            var response = await this.httpClient.GetAsync($"conversation/{conversationId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Conversation>(JsonOptions)
                   ?? throw new InvalidOperationException($"Conversation {conversationId} was not found.");
        }

        public async Task<IReadOnlyList<int>> GetParticipantUserIds(int conversationId)
        {
            return await this.httpClient.GetFromJsonAsync<List<int>>(
                       $"conversation/{conversationId}/participants", JsonOptions)
                   ?? new List<int>();
        }

        public async Task<int> CreateConversation(int senderId, int receiverId)
        {
            var response = await this.httpClient.PostAsJsonAsync(
                "conversation",
                new { SenderId = senderId, ReceiverId = receiverId },
                JsonOptions);
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadAsStringAsync();
            return int.Parse(raw);
        }

        public async Task<Message> HandleNewMessage(Message message)
        {
            var response = await this.httpClient.PostAsJsonAsync("conversation/messages", message, JsonOptions);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Message>(JsonOptions)
                   ?? throw new InvalidOperationException("Failed to create message.");
        }

        public async Task<Message?> HandleMessageUpdate(Message message)
        {
            var response = await this.httpClient.PutAsJsonAsync(
                "conversation/messages", message, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Message>(JsonOptions);
        }

        public async Task HandleReadReceipt(ReadReceiptDTO readReceipt)
        {
            var response = await this.httpClient.PostAsJsonAsync(
                "conversation/readreceipt", readReceipt, JsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Message?> HandleRentalRequestFinalization(int messageId)
        {
            var response = await this.httpClient.PostAsync(
                $"conversation/rental/finalize/{messageId}", null);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Message>(JsonOptions);
        }

        public async Task<Message?> CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            var response = await this.httpClient.PostAsync(
                $"conversation/cash/{messageIdOfParentRentalRequestMessage}/{paymentId}", null);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Message>(JsonOptions);
        }
    }
}

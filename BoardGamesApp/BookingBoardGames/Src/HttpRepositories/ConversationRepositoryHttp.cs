using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using BookingBoardGames.Data;
using BookingBoardGames.Data.DTO;
using BookingBoardGames.Data.Repositories;

namespace BookingBoardGames.Src.HttpRepositories
{
    public class ConversationRepositoryHttp : IConversationRepository
    {
        private readonly HttpClient http;
        private const string BaseUrl = "https://localhost:5001/api/conversation";

        public ConversationRepositoryHttp()
        {
            this.http = new HttpClient();
        }

        public List<Conversation> GetConversationsForUser(int userId)
        {
            return this.http.GetFromJsonAsync<List<Conversation>>(
                $"api/conversation/user/{userId}")
                .GetAwaiter().GetResult() ?? new List<Conversation>();
        }

        public Conversation GetConversationById(int conversationId)
        {
            var result = this.http.GetFromJsonAsync<Conversation>(
                $"api/conversation/{conversationId}")
                .GetAwaiter().GetResult();

            if (result == null)
                throw new Exception("Conversation not found");

            return result;
        }

        public IReadOnlyList<int> GetParticipantUserIds(int conversationId)
        {
            return this.http.GetFromJsonAsync<List<int>>(
                $"api/conversation/participants/{conversationId}")
                .GetAwaiter().GetResult() ?? new List<int>();
        }

        public int CreateConversation(int senderId, int receiverId)
        {
            var response = this.http.PostAsync(
                $"api/conversation/create?senderId={senderId}&receiverId={receiverId}",
                null)
                .GetAwaiter().GetResult();

            return response.Content.ReadFromJsonAsync<int>()
                .GetAwaiter().GetResult();
        }

        public Message HandleNewMessage(Message message)
        {
            var response = this.http.PostAsJsonAsync(
                "api/conversation/message", message)
                .GetAwaiter().GetResult();

            return response.Content.ReadFromJsonAsync<Message>()
                .GetAwaiter().GetResult();
        }

        public Message? HandleMessageUpdate(Message message)
        {
            var response = this.http.PutAsJsonAsync(
                "api/conversation/message", message)
                .GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
                return null;

            return response.Content.ReadFromJsonAsync<Message>()
                .GetAwaiter().GetResult();
        }

        public void HandleReadReceipt(ReadReceiptDTO readReceipt)
        {
            this.http.PostAsJsonAsync("api/conversation/read", readReceipt)
                .GetAwaiter().GetResult();
        }

        public Message? HandleRentalRequestFinalization(int messageId)
        {
            var response = this.http.PostAsync(
                $"api/conversation/rental/finalize/{messageId}", null)
                .GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
                return null;

            return response.Content.ReadFromJsonAsync<Message>()
                .GetAwaiter().GetResult();
        }

        public Message? CreateCashAgreementMessage(int parentId, int paymentId)
        {
            var response = this.http.PostAsync(
                $"api/conversation/cash?parentMessageId={parentId}&paymentId={paymentId}",
                null)
                .GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
                return null;

            return response.Content.ReadFromJsonAsync<Message>()
                .GetAwaiter().GetResult();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using BookingBoardGames.Src.Repositories;

namespace BookingBoardGames.Src.Repositories
{
    public class RepositoryPaymentHttp : IRepositoryPayment
    {
        private readonly HttpClient client;
        private const string BaseUrl = "https://localhost:5001/api/payments/history";

        public RepositoryPaymentHttp()
        {
            this.client = new HttpClient();
        }

        // ------------------------
        // GET ALL HISTORY PAYMENTS
        // ------------------------

        public IReadOnlyList<HistoryPayment> GetAllPayments()
        {
            var response = client.GetAsync(BaseUrl).Result;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<List<HistoryPayment>>(
                       json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                   )
                   ?? new List<HistoryPayment>();
        }

        // ------------------------
        // GET BY ID
        // ------------------------

        public HistoryPayment? GetPaymentById(int searchedPaymentId)
        {
            var response = client.GetAsync($"{BaseUrl}/{searchedPaymentId}").Result;

            if (!response.IsSuccessStatusCode)
                return null;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<HistoryPayment>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}

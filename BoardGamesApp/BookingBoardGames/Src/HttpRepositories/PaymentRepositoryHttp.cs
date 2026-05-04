using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using BookingBoardGames.Src.Repositories;

namespace BookingBoardGames.Src.Repositories
{
    public class PaymentRepositoryHttp : IPaymentRepository
    {
        private readonly HttpClient client;
        private const string BaseUrl = "https://localhost:5001/api/payments";

        public PaymentRepositoryHttp()
        {
            this.client = new HttpClient();
        }

        // ------------------------
        // GET ALL
        // ------------------------

        public IReadOnlyList<Payment> GetAllPayments()
        {
            var response = client.GetAsync(BaseUrl).Result;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<List<Payment>>(
                       json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                   )
                   ?? new List<Payment>();
        }

        // ------------------------
        // GET BY ID
        // ------------------------

        public Payment? GetPaymentByIdentifier(int paymentId)
        {
            var response = client.GetAsync($"{BaseUrl}/{paymentId}").Result;

            if (!response.IsSuccessStatusCode)
                return null;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<Payment>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        // ------------------------
        // ADD
        // ------------------------

        public int AddPayment(Payment payment)
        {
            var json = JsonSerializer.Serialize(payment);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = client.PostAsync(BaseUrl, content).Result;

            var resultJson = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<int>(resultJson);
        }

        // ------------------------
        // DELETE
        // ------------------------

        public bool DeletePayment(Payment payment)
        {
            var response = client
                .DeleteAsync($"{BaseUrl}/{payment.TransactionIdentifier}")
                .Result;

            return response.IsSuccessStatusCode;
        }

        // ------------------------
        // UPDATE
        // ------------------------

        public Payment? UpdatePayment(Payment payment)
        {
            var json = JsonSerializer.Serialize(payment);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = client
                .PutAsync($"{BaseUrl}/{payment.TransactionIdentifier}", content)
                .Result;

            if (!response.IsSuccessStatusCode)
                return null;

            var resultJson = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<Payment>(
                resultJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using BookingBoardGames.Data.Repositories;

namespace BookingBoardGames.Src.HttpRepositories
{
    public class RentalRepositoryHttp : IRentalRepository
    {
        private readonly HttpClient client;
        private const string BaseUrl = "https://localhost:5001/api/rentals";

        public RentalRepositoryHttp()
        {
            this.client = new HttpClient();
        }

        public Rental? GetById(int id)
        {
            var response = client.GetAsync($"{BaseUrl}/{id}").Result;

            if (!response.IsSuccessStatusCode)
                return null;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<Rental>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public TimeRange? GetRentalTimeRange(int id)
        {
            var response = client.GetAsync($"{BaseUrl}/{id}/timerange").Result;

            if (!response.IsSuccessStatusCode)
                return null;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<TimeRange>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public List<TimeRange> GetAllOccupiedPeriods()
        {
            var response = client.GetAsync($"{BaseUrl}/occupied").Result;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<List<TimeRange>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<TimeRange>();
        }

        public List<TimeRange> GetUnavailableTimeRanges(int gameId)
        {
            var response = client.GetAsync($"{BaseUrl}/unavailable/{gameId}").Result;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<List<TimeRange>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<TimeRange>();
        }

        public bool CheckGameAvailability(DateTime start, DateTime end, int gameId)
        {
            var url = $"{BaseUrl}/check?start={start:o}&end={end:o}&gameId={gameId}";

            var response = client.GetAsync(url).Result;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<bool>(json);
        }

        public void AddRental(Rental rental)
        {
            var json = JsonSerializer.Serialize(rental);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            client.PostAsync(BaseUrl, content).Wait();
        }
    }
}

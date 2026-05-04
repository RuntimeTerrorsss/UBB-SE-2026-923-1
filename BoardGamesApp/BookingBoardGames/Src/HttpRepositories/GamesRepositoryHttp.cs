using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Shared;

namespace BookingBoardGames.Src.Repositories
{
    public class GamesRepositoryHttp : InterfaceGamesRepository
    {
        private readonly HttpClient client;
        private const string BaseUrl = "https://localhost:5001/api/games";

        public GamesRepositoryHttp()
        {
            this.client = new HttpClient();
        }

        // ------------------------
        // BASIC METHODS
        // ------------------------

        public Game? GetGameById(int gameId)
        {
            var response = client.GetAsync($"{BaseUrl}/{gameId}").Result;

            if (!response.IsSuccessStatusCode)
                return null;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<Game>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public decimal GetPriceGameById(int gameId)
        {
            var response = client.GetAsync($"{BaseUrl}/{gameId}/price").Result;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<decimal>(json);
        }

        public List<Game> GetAll()
        {
            var response = client.GetAsync($"{BaseUrl}/all").Result;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<List<Game>>(json)
                   ?? new List<Game>();
        }

        // ------------------------
        // FILTER
        // ------------------------

        public List<Game> GetGamesByFilter(FilterCriteria filter)
        {
            var jsonFilter = JsonSerializer.Serialize(filter);

            var content = new StringContent(
                jsonFilter,
                System.Text.Encoding.UTF8,
                "application/json");

            var response = client.PostAsync($"{BaseUrl}/filter", content).Result;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<List<Game>>(json)
                   ?? new List<Game>();
        }

        // ------------------------
        // FEED METHODS
        // ------------------------

        public List<Game> GetGamesForFeedAvailableTonight(int userId)
        {
            var response = client.GetAsync($"{BaseUrl}/feed/available-tonight/{userId}").Result;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<List<Game>>(json)
                   ?? new List<Game>();
        }

        public List<Game> GetRemainingGamesForFeed(int userId)
        {
            var response = client.GetAsync($"{BaseUrl}/feed/remaining/{userId}").Result;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<List<Game>>(json)
                   ?? new List<Game>();
        }
    }
}

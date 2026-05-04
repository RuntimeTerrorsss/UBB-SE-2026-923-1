using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using BookingBoardGames.Src.Repositories;

namespace BookingBoardGames.Src.Repositories
{
    public class UserRepositoryHttp : IUserRepository
    {
        private readonly HttpClient client;
        private const string BaseUrl = "https://localhost:5001/api/users";

        public UserRepositoryHttp()
        {
            this.client = new HttpClient();
        }

        // ------------------------
        // GET BY ID
        // ------------------------

        public User? GetById(int id)
        {
            var response = client.GetAsync($"{BaseUrl}/{id}").Result;

            if (!response.IsSuccessStatusCode)
                return null;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<User>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        // ------------------------
        // SAVE ADDRESS
        // ------------------------

        public void SaveAddress(int id, Address address)
        {
            var json = JsonSerializer.Serialize(address);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            client.PutAsync($"{BaseUrl}/{id}/address", content).Wait();
        }

        // ------------------------
        // GET BALANCE
        // ------------------------

        public decimal GetUserBalance(int userId)
        {
            var response = client.GetAsync($"{BaseUrl}/{userId}/balance").Result;

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<decimal>(json);
        }

        // ------------------------
        // UPDATE BALANCE
        // ------------------------

        public void UpdateBalance(int userId, decimal newBalance)
        {
            var json = JsonSerializer.Serialize(newBalance);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            client.PutAsync($"{BaseUrl}/{userId}/balance", content).Wait();
        }

        public List<User> GetAll()
        {
            throw new NotImplementedException();
        }

        public User? GetGameById(int id)
        {
            throw new NotImplementedException();
        }
    }
}

// <copyright file="UserAPIProxy.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Interfaces;

namespace BookingBoardGames.Src.Repositories
{
    public class UserAPIProxy : IUserRepository
    {
        private readonly HttpClient httpClient;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public UserAPIProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<User?> GetById(int id)
        {
            var response = await this.httpClient.GetAsync($"api/users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<User>(JsonOptions);
        }

        public async Task<User?> GetGameById(int id)
        {
            return await this.GetById(id);
        }

        public async Task<List<User>> GetAll()
        {
            return await this.httpClient.GetFromJsonAsync<List<User>>("api/users", JsonOptions)
                   ?? new List<User>();
        }

        public async Task SaveAddress(int id, Address address)
        {
            var response = await this.httpClient.PutAsJsonAsync($"api/users/{id}/address", address, JsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task<decimal> GetUserBalance(int userId)
        {
            var response = await this.httpClient.GetAsync($"api/users/{userId}/balance");
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadAsStringAsync();
            return decimal.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
        }

        public async Task UpdateBalance(int userId, decimal newBalance)
        {
            var response = await this.httpClient.PutAsJsonAsync($"api/users/{userId}/balance", newBalance, JsonOptions);
            response.EnsureSuccessStatusCode();
        }
    }
}

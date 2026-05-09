// <copyright file="RentalRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Interfaces;

namespace BookingBoardGames.Src.Repositories
{
    public class RentalAPIProxy : IRentalRepository
    {
        private readonly HttpClient httpClient;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public RentalAPIProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Rental?> GetById(int rentalId)
        {
            var response = await this.httpClient.GetAsync($"rentals/{rentalId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Rental>(JsonOptions);
        }

        public async Task<TimeRange?> GetRentalTimeRange(int rentalId)
        {
            var response = await this.httpClient.GetAsync($"rentals/{rentalId}/timerange");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TimeRange>(JsonOptions);
        }

        public async Task<List<TimeRange>> GetAllOccupiedPeriods()
        {
            var response = await this.httpClient.GetAsync("rentals/occupied");
            if (!response.IsSuccessStatusCode)
            {
                return new List<TimeRange>();
            }

            return await response.Content.ReadFromJsonAsync<List<TimeRange>>(JsonOptions)
                   ?? new List<TimeRange>();
        }

        public async Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId)
        {
            return await this.httpClient.GetFromJsonAsync<List<TimeRange>>(
                       $"rentals/game/{gameId}/unavailable", JsonOptions)
                   ?? new List<TimeRange>();
        }

        public async Task<bool> CheckGameAvailability(DateTime startTime, DateTime endTime, int gameId)
        {
            var range = new TimeRange(startTime, endTime);
            var response = await this.httpClient.PostAsJsonAsync($"rentals/{gameId}/check", range, JsonOptions);
            response.EnsureSuccessStatusCode();
            var available = await response.Content.ReadFromJsonAsync<bool>(JsonOptions);
            return available;
        }

        public async Task AddRental(Rental rental)
        {
            var response = await this.httpClient.PostAsJsonAsync("rentals", rental, JsonOptions);
            response.EnsureSuccessStatusCode();
        }
    }
}

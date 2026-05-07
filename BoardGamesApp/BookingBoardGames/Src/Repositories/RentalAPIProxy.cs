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
        var response = await this.httpClient.GetAsync($"api/rentals/{rentalId}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<Rental>(JsonOptions);
    }

    public async Task<TimeRange?> GetRentalTimeRange(int rentalId)
    {
        var response = await this.httpClient.GetAsync($"api/rentals/{rentalId}/timerange");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<TimeRange>(JsonOptions);
    }

    public async Task<List<TimeRange>> GetAllOccupiedPeriods()
    {
        return await this.httpClient.GetFromJsonAsync<List<TimeRange>>("api/rentals/occupied", JsonOptions)
               ?? new List<TimeRange>();
    }

    public async Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId)
    {
        return await this.httpClient.GetFromJsonAsync<List<TimeRange>>(
                   $"api/rentals/unavailable/{gameId}", JsonOptions)
               ?? new List<TimeRange>();
    }

    public async Task<bool> CheckGameAvailability(DateTime startTime, DateTime endTime, int gameId)
    {
        var response = await this.httpClient.GetAsync(
            $"api/rentals/availability?gameId={gameId}&start={startTime:O}&end={endTime:O}");
        response.EnsureSuccessStatusCode();
        var raw = await response.Content.ReadAsStringAsync();
        return bool.Parse(raw);
    }

    public async Task AddRental(Rental rental)
    {
        var response = await this.httpClient.PostAsJsonAsync("api/rentals", rental, JsonOptions);
        response.EnsureSuccessStatusCode();
    }
}

// <copyright file="IRentalService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingBoardGames.Src.Services
{
    public interface IRentalService
    {
        public Rental GetRentalById(int rentalId);

        public Task<decimal> GetRentalPrice(int rentalId);

        public Task<string> GetGameName(int rentalId);

        public List<TimeRange> GetUnavailableTimeRanges(int gameId);

        public bool CheckGameAvailability(int gameId, DateTime startDate, DateTime endDate);

        public decimal CalculateTotalPriceForRentingASpecificGame(decimal price, TimeRange timeRange);

        public int CalculateNumberOfDaysInAGivenTimeRange(TimeRange selectedTimeRange);

        public Task<Rental> CreateRental(int gameId, int clientId, int ownerId, DateTime startDate, DateTime endDate);
    }
}

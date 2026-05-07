// <copyright file="IRentalRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace BookingBoardGames.Data.Interfaces
{
    public interface IRentalRepository
    {
        Task<Rental?> GetById(int id);

        Task<TimeRange?> GetRentalTimeRange(int id);

        Task<List<TimeRange>> GetAllOccupiedPeriods();

        Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId);

        Task<bool> CheckGameAvailability(DateTime start, DateTime end, int gameId);

        Task AddRental(Rental rental);
    }
}

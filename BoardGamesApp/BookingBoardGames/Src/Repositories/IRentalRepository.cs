// <copyright file="IRentalRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace BookingBoardGames.Src.Repositories
{
    public interface IRentalRepository
    {
        public Rental? GetById(int id);

        public TimeRange? GetRentalTimeRange(int id);

        public List<TimeRange> GetAllOccupiedPeriods();

        public List<TimeRange> GetUnavailableTimeRanges(int gameId);

        public bool CheckGameAvailability(DateTime start, DateTime end, int gameId);

        public void AddRental(Rental rental);
    }
}

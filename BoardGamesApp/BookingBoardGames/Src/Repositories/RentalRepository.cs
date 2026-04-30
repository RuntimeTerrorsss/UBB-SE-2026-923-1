// <copyright file="RentalRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;

namespace BookingBoardGames.Src.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly AppDbContext context;

        public RentalRepository(AppDbContext appContext)
        {
            this.context = appContext;
        }

        public Rental? GetById(int id)
        {
            return this.context.Rentals.FirstOrDefault(r => r.RentalId == id);
        }

        public TimeRange? GetRentalTimeRange(int id)
        {
            return this.context.Rentals
                .Where(r => r.RentalId == id)
                .Select(r => new TimeRange(r.StartDate, r.EndDate))
                .FirstOrDefault();
        }

        public List<TimeRange> GetAllOccupiedPeriods()
        {
            return this.context.Rentals
                .Select(r => new TimeRange(r.StartDate, r.EndDate))
                .ToList();
        }

        public List<TimeRange> GetUnavailableTimeRanges(int gameId)
        {
            return this.context.Rentals
                .Where(r => r.GameId == gameId)
                .Select(r => new TimeRange(r.StartDate, r.EndDate))
                .ToList();
        }

        public bool CheckGameAvailability(DateTime start, DateTime end, int gameId)
        {
            bool hasOverlap = this.context.Rentals.Any(r =>
                r.GameId == gameId &&
                r.StartDate < end &&
                start < r.EndDate);

            return !hasOverlap;
        }

        public void AddRental(Rental rental)
        {
            this.context.Rentals.Add(rental);
            this.context.SaveChanges();
        }
    }
}

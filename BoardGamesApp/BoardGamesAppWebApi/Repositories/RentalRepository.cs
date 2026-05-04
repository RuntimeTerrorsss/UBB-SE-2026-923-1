// <copyright file="RentalRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using BoardGamesAppWebApi.Data;

namespace BookingBoardGames.Src.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly AppDbContext context;

        public RentalRepository(AppDbContext appContext)
        {
            this.context = appContext;
        }

        public Rental? GetById(int rentalId)
        {
            return this.context.Rentals.FirstOrDefault(rental => rental.RentalId == rentalId);
        }

        public TimeRange? GetRentalTimeRange(int rentalId)
        {
            return this.context.Rentals
                .Where(rental => rental.RentalId == rentalId)
                .Select(rental => new TimeRange(rental.StartDate, rental.EndDate))
                .FirstOrDefault();
        }

        public List<TimeRange> GetAllOccupiedPeriods()
        {
            return this.context.Rentals
                .Select(rental => new TimeRange(rental.StartDate, rental.EndDate))
                .ToList();
        }

        public List<TimeRange> GetUnavailableTimeRanges(int gameId)
        {
            return this.context.Rentals
                .Where(rental => rental.GameId == gameId)
                .Select(rental => new TimeRange(rental.StartDate, rental.EndDate))
                .ToList();
        }

        public bool CheckGameAvailability(DateTime startTime, DateTime endTime, int gameId)
        {
            bool hasOverlap = this.context.Rentals.Any(rental =>
                rental.GameId == gameId &&
                rental.StartDate < endTime &&
                startTime < rental.EndDate);

            return !hasOverlap;
        }

        public void AddRental(Rental rental)
        {
            this.context.Rentals.Add(rental);
            this.context.SaveChanges();
        }
    }
}

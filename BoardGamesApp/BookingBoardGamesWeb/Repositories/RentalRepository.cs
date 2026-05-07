// <copyright file="RentalRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BookingBoardGames.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingBoardGames.Data.Interfaces
{
    public class RentalRepository : IRentalRepository
    {
        private readonly AppDbContext context;

        public RentalRepository(AppDbContext appContext)
        {
            this.context = appContext;
        }

        public async Task<Rental?> GetById(int rentalId)
        {
            return await this.context.Rentals
                .FirstOrDefaultAsync(rental => rental.RentalId == rentalId);
        }

        public async Task<TimeRange?> GetRentalTimeRange(int rentalId)
        {
            return await this.context.Rentals
                .Where(rental => rental.RentalId == rentalId)
                .Select(rental => new TimeRange(rental.StartDate, rental.EndDate))
                .FirstOrDefaultAsync();
        }

        public async Task<List<TimeRange>> GetAllOccupiedPeriods()
        {
            return await this.context.Rentals
                .Select(rental => new TimeRange(rental.StartDate, rental.EndDate))
                .ToListAsync();
        }

        public async Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId)
        {
            return await this.context.Rentals
                .Where(rental => rental.GameId == gameId)
                .Select(rental => new TimeRange(rental.StartDate, rental.EndDate))
                .ToListAsync();
        }

        public async Task<bool> CheckGameAvailability(DateTime startTime, DateTime endTime, int gameId)
        {
            bool hasOverlap = await this.context.Rentals.AnyAsync(rental =>
                rental.GameId == gameId &&
                rental.StartDate < endTime &&
                startTime < rental.EndDate);
            return !hasOverlap;
        }

        public async Task AddRental(Rental rental)
        {
            await this.context.Rentals.AddAsync(rental);
            await this.context.SaveChangesAsync();
        }
    }
}

// <copyright file="RentalService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Data.Interfaces;

namespace BookingBoardGames.Data.Services
{
    public class RentalService : IRentalService
    {
        private const int MinimumValidDayCount = 1;

        private readonly IRentalRepository rentalRepository;
        private readonly InterfaceGamesRepository gameRepository;

        public RentalService(IRentalRepository rentalRepository, InterfaceGamesRepository gameRepository)
        {
            this.rentalRepository = rentalRepository;
            this.gameRepository = gameRepository;
        }

        public async Task<Rental> GetRentalById(int rentalId)
        {
            return await this.rentalRepository.GetById(rentalId);
        }

        public async Task<decimal> GetRentalPrice(int rentalId)
        {
            var rental = await this.rentalRepository.GetById(rentalId);

            if (rental == null)
            {
                return 0m;
            }

            var pricePerDay = await this.gameRepository.GetPriceGameById(rental.GameId);
            var timeRange = new TimeRange(rental.StartDate, rental.EndDate);

            return await this.CalculateTotalPriceForRentingASpecificGame(pricePerDay, timeRange);
        }

        public async Task<string> GetGameName(int rentalId)
        {
            var rental = await this.rentalRepository.GetById(rentalId);

            if (rental == null)
            {
                return "Unknown Rental";
            }

            var game = await this.gameRepository.GetGameById(rental.GameId);

            if (game == null)
            {
                return "Unknown Game";
            }

            return game.Name;
        }

        public async Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId)
        {
            return await this.rentalRepository.GetUnavailableTimeRanges(gameId);
        }

        public async Task<bool> CheckGameAvailability(int gameId, DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                return false;
            }

            return await this.rentalRepository.CheckGameAvailability(startDate, endDate, gameId);
        }

        public async Task<decimal> CalculateTotalPriceForRentingASpecificGame(decimal price, TimeRange timeRange)
        {
            int days = await this.CalculateNumberOfDaysInAGivenTimeRange(timeRange);
            return days * price;
        }

        public async Task<int> CalculateNumberOfDaysInAGivenTimeRange(TimeRange selectedTimeRange)
        {
            int days = (selectedTimeRange.EndTime - selectedTimeRange.StartTime).Days + MinimumValidDayCount;
            return days < MinimumValidDayCount ? MinimumValidDayCount : days;
        }

        public async Task<Rental> CreateRental(int gameId, int clientId, int ownerId, DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                throw new ArgumentException("End date must be after start date.");
            }

            bool isAvailable = await this.CheckGameAvailability(gameId, startDate, endDate);

            if (!isAvailable)
            {
                throw new InvalidOperationException("The game is not available for the selected period.");
            }

            var pricePerDay = await this.gameRepository.GetPriceGameById(gameId);
            var timeRange = new TimeRange(startDate, endDate);
            var totalPrice = await this.CalculateTotalPriceForRentingASpecificGame(pricePerDay, timeRange);

            var rental = new Rental
            {
                GameId = gameId,
                ClientId = clientId,
                OwnerId = ownerId,
                StartDate = startDate,
                EndDate = endDate,
                TotalPrice = totalPrice,
            };

            await this.rentalRepository.AddRental(rental);

            return rental;
        }
    }
}

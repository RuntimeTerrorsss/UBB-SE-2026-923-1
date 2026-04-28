using System;
using System.Collections.Generic;
using BookingBoardGames.Src.Repositories;

namespace BookingBoardGames.Src.Services
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

        public Rental GetRentalById(int rentalId)
        {
            return this.rentalRepository.GetById(rentalId);
        }

        public decimal GetRentalPrice(int rentalId)
        {
            var rental = this.rentalRepository.GetById(rentalId);

            if (rental == null)
            {
                return 0m;
            }

            var pricePerDay = this.gameRepository.GetPriceGameById(rental.GameId);
            var timeRange = new TimeRange(rental.StartDate, rental.EndDate);

            return this.CalculateTotalPriceForRentingASpecificGame(pricePerDay, timeRange);
        }

        public string GetGameName(int rentalId)
        {
            var rental = this.rentalRepository.GetById(rentalId);

            if (rental == null)
            {
                return "Unknown Rental";
            }

            var game = this.gameRepository.GetGameById(rental.GameId);

            if (game == null)
            {
                return "Unknown Game";
            }

            return game.Name;
        }

        public List<TimeRange> GetUnavailableTimeRanges(int gameId)
        {
            return this.rentalRepository.GetUnavailableTimeRanges(gameId);
        }

        public bool CheckGameAvailability(int gameId, DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                return false;
            }

            return this.rentalRepository.CheckGameAvailability(startDate, endDate, gameId);
        }

        public decimal CalculateTotalPriceForRentingASpecificGame(decimal price, TimeRange timeRange)
        {
            int days = this.CalculateNumberOfDaysInAGivenTimeRange(timeRange);
            return days * price;
        }

        public int CalculateNumberOfDaysInAGivenTimeRange(TimeRange selectedTimeRange)
        {
            int days = (selectedTimeRange.EndTime - selectedTimeRange.StartTime).Days + MinimumValidDayCount;
            return days < MinimumValidDayCount ? MinimumValidDayCount : days;
        }

        public Rental CreateRental(int gameId, int clientId, int ownerId, DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                throw new ArgumentException("End date must be after start date.");
            }

            bool isAvailable = this.CheckGameAvailability(gameId, startDate, endDate);

            if (!isAvailable)
            {
                throw new InvalidOperationException("The game is not available for the selected period.");
            }

            var pricePerDay = this.gameRepository.GetPriceGameById(gameId);
            var timeRange = new TimeRange(startDate, endDate);
            var totalPrice = this.CalculateTotalPriceForRentingASpecificGame(pricePerDay, timeRange);

            var rental = new Rental
            {
                GameId = gameId,
                ClientId = clientId,
                OwnerId = ownerId,
                StartDate = startDate,
                EndDate = endDate,
                TotalPrice = totalPrice
            };

            this.rentalRepository.AddRental(rental);

            return rental;
        }
    }
}

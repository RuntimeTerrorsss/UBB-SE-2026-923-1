using System;
using System.Collections.Generic;

namespace BookingBoardGames.Src.Services
{
    public interface IRentalService
    {
        public Rental GetRentalById(int rentalId);

        public decimal GetRentalPrice(int rentalId);

        public string GetGameName(int rentalId);

        public List<TimeRange> GetUnavailableTimeRanges(int gameId);

        public bool CheckGameAvailability(int gameId, DateTime startDate, DateTime endDate);

        public decimal CalculateTotalPriceForRentingASpecificGame(decimal price, TimeRange timeRange);

        public int CalculateNumberOfDaysInAGivenTimeRange(TimeRange selectedTimeRange);

        public Rental CreateRental(int gameId, int clientId, int ownerId, DateTime startDate, DateTime endDate);
    }
}

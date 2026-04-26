using System;
using System.Collections.Generic;

namespace BookingBoardGames.Src.Repositories
{
    public interface IRequestRepository
    {
        public Rental GetById(int id);

        public List<TimeRange> GetAllOccupiedPeriods();

        public List<TimeRange> GetUnavailableTimeRanges(int gameId);

        public bool CheckGameAvailability(DateTime start, DateTime end, int gameId);
    }
}

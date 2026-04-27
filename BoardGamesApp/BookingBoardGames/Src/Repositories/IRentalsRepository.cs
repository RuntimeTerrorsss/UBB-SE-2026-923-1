using System.Collections.Generic;
using BookingBoardGames.Repositories;
using BookingBoardGames.Src.Models;
using SearchAndBook.Repositories;

namespace SearchAndBook.Repositories
{
    public interface IRentalsRepository : IRepository<Rental>
    {
        new TimeRange? GetGameById(int id);

        new List<TimeRange> GetAll();

        List<TimeRange> GetUnavailableTimeRanges(int gameId);

        bool CheckGameAvailability(TimeRange range, int gameId);
    }
}

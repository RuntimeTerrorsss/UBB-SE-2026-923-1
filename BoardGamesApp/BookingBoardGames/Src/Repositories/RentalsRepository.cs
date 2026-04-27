using System.Collections.Generic;
using System.Linq;
using BookingBoardGames;
using BookingBoardGames.Repositories;
using BookingBoardGames.Src.Models;
using SearchAndBook.Shared;

namespace SearchAndBook.Repositories
{
    /// <summary>
    /// Repository for managing rental data using Entity Framework Core.
    /// </summary>
    public class RentalsRepository : IRentalsRepository
    {
        private readonly AppDbContextFactory contextFactory = new();

        /// <summary>
        /// Retrieves a rental time range by its unique identifier.
        /// </summary>
        public TimeRange? GetGameById(int id)
        {
            using var context = this.contextFactory.CreateDbContext([]);
            var rental = context.Rentals.FirstOrDefault(r => r.RentalId == id);

            if (rental is null)
            {
                return null;
            }

            return new TimeRange(rental.StartDate, rental.EndDate);
        }

        /// <summary>
        /// Retrieves all rental time ranges.
        /// </summary>
        public List<TimeRange> GetAll()
        {
            using var context = this.contextFactory.CreateDbContext([]);
            return context.Rentals
                .Select(r => new TimeRange(r.StartDate, r.EndDate))
                .ToList();
        }

        /// <summary>
        /// Retrieves unavailable rental time ranges for a specific game.
        /// </summary>
        public List<TimeRange> GetUnavailableTimeRanges(int gameId)
        {
            using var context = this.contextFactory.CreateDbContext([]);
            return context.Rentals
                .Where(r => r.GameId == gameId)
                .Select(r => new TimeRange(r.StartDate, r.EndDate))
                .ToList();
        }

        /// <summary>
        /// Checks if a game is available for a specified time range.
        /// </summary>
        public bool CheckGameAvailability(TimeRange range, int gameId)
        {
            using var context = this.contextFactory.CreateDbContext([]);
            bool hasOverlap = context.Rentals.Any(r =>
                r.GameId == gameId &&
                r.StartDate < range.EndTime &&
                r.EndDate > range.StartTime);

            return !hasOverlap;
        }

        Rental? IRepository<Rental>.GetGameById(int id)
        {
            throw new System.NotImplementedException();
        }

        List<Rental> IRepository<Rental>.GetAll()
        {
            throw new System.NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;

namespace BookingBoardGames.Src.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly AppDbContext _context;

        public RentalRepository(AppDbContext context)
        {
            _context = context;
        }

        public Rental GetById(int id)
        {
            return _context.Rentals
                .FirstOrDefault(r => r.RentalId == id);
        }

        public TimeRange? GetRentalTimeRange(int id)
        {
            return _context.Rentals
                .Where(r => r.RentalId == id)
                .Select(r => new TimeRange(r.StartDate, r.EndDate))
                .FirstOrDefault();
        }

        public List<TimeRange> GetAllOccupiedPeriods()
        {
            return _context.Rentals
                .Select(r => new TimeRange(r.StartDate, r.EndDate))
                .ToList();
        }

        public List<TimeRange> GetUnavailableTimeRanges(int gameId)
        {
            return _context.Rentals
                .Where(r => r.GameId == gameId)
                .Select(r => new TimeRange(r.StartDate, r.EndDate))
                .ToList();
        }

        public bool CheckGameAvailability(DateTime start, DateTime end, int gameId)
        {
            bool hasOverlap = _context.Rentals.Any(r =>
                r.GameId == gameId &&
                r.StartDate < end &&
                start < r.EndDate);

            return !hasOverlap;
        }

        public void AddRental(Rental rental)
        {
            this._context.Rentals.Add(rental);
            this._context.SaveChanges();
        }
    }
}

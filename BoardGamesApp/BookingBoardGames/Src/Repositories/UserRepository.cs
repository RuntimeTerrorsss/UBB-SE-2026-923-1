using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Src.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            this._context = context;
        }
        public User? GetById(int id)
        {
            return _context.Users.FirstOrDefault(user => user.Id == id);
        }

        public User? GetGameById(int id)
        {
            return GetById(id);
        }

        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }

        public void SaveAddress(int id, Address address)
        {
            var foundUser = _context.Users.FirstOrDefault(user => user.Id == id);

            if (foundUser is null)
            {
                return;
            }

            foundUser.Country = address.Country;
            foundUser.City = address.City;
            foundUser.Street = address.Street;
            foundUser.StreetNumber = address.StreetNumber;
            _context.SaveChanges();
        }

        public decimal GetUserBalance(int userId)
        {
            return _context.Users
                .Where(user => user.Id == userId)
                .Select(user => (decimal?)user.Balance)
                .FirstOrDefault() ?? 0m;
        }

        public void UpdateBalance(int userId, decimal newBalance)
        {
            var foundUser = _context.Users.FirstOrDefault(user => user.Id == userId);

            if (foundUser is null)
            {
                return;
            }

            foundUser.Balance = newBalance;
            _context.SaveChanges();
        }
    }
}

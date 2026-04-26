using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Src.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Src.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContextFactory contextFactory = new();

        public User? GetById(int id)
        {
            using var context = contextFactory.CreateDbContext([]);
            return context.Users.FirstOrDefault(user => user.UserId == id);
        }

        public User? GetGameById(int id)
        {
            return GetById(id);
        }

        public List<User> GetAll()
        {
            using var context = contextFactory.CreateDbContext([]);
            return context.Users.ToList();
        }

        public void SaveAddress(int id, Address address)
        {
            using var context = contextFactory.CreateDbContext([]);
            var foundUser = context.Users.FirstOrDefault(user => user.UserId == id);

            if (foundUser is null)
            {
                return;
            }

            foundUser.Country = address.Country;
            foundUser.City = address.City;
            foundUser.Street = address.Street;
            foundUser.StreetNumber = address.StreetNumber;
            context.SaveChanges();
        }

        public decimal GetUserBalance(int userId)
        {
            using var context = contextFactory.CreateDbContext([]);
            return context.Users
                .Where(user => user.UserId == userId)
                .Select(user => (decimal?)user.Balance)
                .FirstOrDefault() ?? 0m;
        }

        public void UpdateBalance(int userId, decimal newBalance)
        {
            using var context = contextFactory.CreateDbContext([]);
            var foundUser = context.Users.FirstOrDefault(user => user.UserId == userId);

            if (foundUser is null)
            {
                return;
            }

            foundUser.Balance = newBalance;
            context.SaveChanges();
        }
    }
}

// <copyright file="UserRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Data.Interfaces
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext context;

        public UserRepository(AppDbContext appContext)
        {
            this.context = appContext;
        }

        public User? GetById(int id)
        {
            return this.context.Users.FirstOrDefault(user => user.Id == id);
        }

        public User? GetGameById(int id)
        {
            return this.GetById(id);
        }

        public List<User> GetAll()
        {
            return this.context.Users.ToList();
        }

        public void SaveAddress(int id, Address address)
        {
            var foundUser = this.context.Users.FirstOrDefault(user => user.Id == id);

            if (foundUser is null)
            {
                return;
            }

            foundUser.Country = address.Country;
            foundUser.City = address.City;
            foundUser.Street = address.Street;
            foundUser.StreetNumber = address.StreetNumber;
            this.context.SaveChanges();
        }

        public decimal GetUserBalance(int userId)
        {
            return this.context.Users
                .Where(user => user.Id == userId)
                .Select(user => (decimal?)user.Balance)
                .FirstOrDefault() ?? 0m;
        }

        public void UpdateBalance(int userId, decimal newBalance)
        {
            var foundUser = this.context.Users.FirstOrDefault(user => user.Id == userId);

            if (foundUser is null)
            {
                return;
            }

            foundUser.Balance = newBalance;
            this.context.SaveChanges();
        }
    }
}

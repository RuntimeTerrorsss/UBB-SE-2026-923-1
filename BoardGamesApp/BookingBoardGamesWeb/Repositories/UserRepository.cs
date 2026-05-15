// <copyright file="UserRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Constants;
using BookingBoardGames.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using BCrypt.Net;

namespace BookingBoardGames.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext context;

        public UserRepository(AppDbContext appContext)
        {
            this.context = appContext;
        }

        public async Task<User?> GetById(int id)
        {
            return await this.context.Users.FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<User?> GetGameById(int id)
        {
            return await this.GetById(id);
        }

        public async Task<List<User>> GetAll()
        {
            return await this.context.Users.ToListAsync();
        }

        public async Task SaveAddress(int id, Address address)
        {
            var foundUser = await this.context.Users.FirstOrDefaultAsync(user => user.Id == id);

            if (foundUser is null)
            {
                return;
            }

            foundUser.Country = address.Country;
            foundUser.City = address.City;
            foundUser.Street = address.Street;
            foundUser.StreetNumber = address.StreetNumber;

            await this.context.SaveChangesAsync();
        }

        public async Task<decimal> GetUserBalance(int userId)
        {
            return await this.context.Users
                .Where(user => user.Id == userId)
                .Select(user => (decimal?)user.Balance)
                .FirstOrDefaultAsync() ?? 0m;
        }

        public async Task UpdateBalance(int userId, decimal newBalance)
        {
            var foundUser = await this.context.Users.FirstOrDefaultAsync(user => user.Id == userId);

            if (foundUser is null)
            {
                return;
            }

            foundUser.Balance = newBalance;

            await this.context.SaveChangesAsync();
        }
        private async Task<User?> GetByIdentifier(string identifier)
        {
            return await this.context.Users
                .FirstOrDefaultAsync(u => u.Email == identifier || u.Username == identifier);
        }

        public async Task<User?> Login(string emailOrUsername, string password)
        {
            var user = await GetByIdentifier(emailOrUsername);

            if (user == null) return null;


            bool isPasswordOk = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return isPasswordOk ? user : null;
        }

        public async Task<bool> Register(User newUser)
        {
            var exists = await this.context.Users
                .AnyAsync(user => user.Username == newUser.Username || user.Email == newUser.Email);

            if (exists) return false;

            newUser.CreatedAt = DateTime.Now;
            await this.context.Users.AddAsync(newUser);

            var result = await this.context.SaveChangesAsync();
            return result > 0;
        }
    }
}
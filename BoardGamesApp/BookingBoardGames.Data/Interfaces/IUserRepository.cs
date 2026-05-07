// <copyright file="IUserRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
using System.Threading.Tasks;

namespace BookingBoardGames.Data.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetById(int id);

        Task SaveAddress(int id, Address address);

        Task<decimal> GetUserBalance(int userId);

        Task UpdateBalance(int userId, decimal newBalance);
    }
}

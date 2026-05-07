// <copyright file="IUserRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BookingBoardGames.Data.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        public User? GetById(int id);

        public void SaveAddress(int id, Address address);

        public decimal GetUserBalance(int userId);

        public void UpdateBalance(int userId, decimal newBalance);
    }
}

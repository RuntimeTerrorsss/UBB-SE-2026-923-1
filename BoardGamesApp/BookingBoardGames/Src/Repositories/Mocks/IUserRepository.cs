using BookingBoardGames.Repositories;
using BookingBoardGames.Src.Models;

namespace BookingBoardGames.Src.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        public User? GetById(int id);
        public void SaveAddress(int id, Address address);
        public decimal GetUserBalance(int userId);
        public void UpdateBalance(int userId, decimal newBalance);
    }
}

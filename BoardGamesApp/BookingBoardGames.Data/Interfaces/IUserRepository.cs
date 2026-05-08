
namespace BookingBoardGames.Data.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        public Task<User?> GetById(int id);

        public Task SaveAddress(int id, Address address);

        public Task<decimal> GetUserBalance(int userId);

        public Task UpdateBalance(int userId, decimal newBalance);
    }
}

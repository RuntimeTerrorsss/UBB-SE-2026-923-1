namespace BookingBoardGames.Src.Repositories.Mocks
{
    public interface IGameRepository
    {
        public Game GetById(int id);

        public decimal GetPriceGameById(int gameId);
    }
}

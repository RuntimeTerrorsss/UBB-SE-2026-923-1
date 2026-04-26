namespace BookingBoardGames.Src.Repositories
{
    public interface IGameRepository
    {
        public Game GetById(int id);

        public decimal GetPriceGameById(int gameId);
    }
}

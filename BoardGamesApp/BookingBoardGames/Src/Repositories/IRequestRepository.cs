namespace BookingBoardGames.Src.Repositories
{
    public interface IRequestRepository
    {
        public Rental GetById(int id);
    }
}

using BookingBoardGames.Src.Models;

namespace BookingBoardGames.Src.Services
{
    public interface IRequestService
    {
        public Rental GetRequestById(int requestId);
        public decimal GetRequestPrice(int requestId);
        public string GetGameName(int requestId);
    }
}

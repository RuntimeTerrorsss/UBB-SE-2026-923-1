using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardGames.Src.Repositories.Mocks;

namespace BookingBoardGames.Src.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository requestRepository;
        private readonly IGameRepository gameRepository;

        public RequestService(IRequestRepository requestRepository, IGameRepository gameRepository)
        {
            this.requestRepository = requestRepository;
            this.gameRepository = gameRepository;
        }

        public Rental GetRequestById(int requestId)
        {
            return requestRepository.GetById(requestId);
        }

        public decimal GetRequestPrice(int requestId)
        {
            var request = requestRepository.GetById(requestId);

            if (request == null)
            {
                return 0m;
            }

            int totalDays = (request.EndDate - request.StartDate).Days;
            int billedDays = Math.Max(1, totalDays);

            var pricePerDay = gameRepository.GetPriceGameById(request.GameId);

            return pricePerDay * billedDays;
        }

        public string GetGameName(int requestId)
        {
            var request = requestRepository.GetById(requestId);
            if (request == null)
            {
                return "Unknown Request";
            }

            var game = gameRepository.GetById(request.GameId);
            if (game == null)
            {
                return "Unknown Game";
            }

            return game.Name;
        }
    }
}

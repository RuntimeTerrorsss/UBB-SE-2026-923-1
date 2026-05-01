// <copyright file="CashPaymentViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;

namespace BookingBoardGames.Src.ViewModels
{
    public class CashPaymentViewModel
    {
        private const int NewPaymentPlaceholderId = -1;
        private const string DateRangeSeparator = " to ";

        private readonly ICashPaymentService cashPaymentService;
        private readonly IUserRepository userRepository;
        private readonly IRentalService rentalRequestService;
        private readonly InterfaceGamesRepository gameRepository;
        private readonly ConversationService conversationService;

        public string OwnerName { get; set; }

        public string GameName { get; set; }

        public string DeliveryAddress { get; set; }

        public string RequestDates { get; set; }

        public string PaidAmount { get; set; }

        private readonly int rentalRequestMessageIdentifier;

        public CashPaymentViewModel(
            ICashPaymentService cashPaymentService,
            IUserRepository userRepository,
            IRentalService rentalRequestService,
            InterfaceGamesRepository gameRepository,
            int rentalRequestId,
            string deliveryAddress,
            int rentalRequestMessageIdentifier,
            ConversationService conversationService)
        {
            this.cashPaymentService = cashPaymentService;
            this.userRepository = userRepository;
            this.rentalRequestService = rentalRequestService;
            this.gameRepository = gameRepository;
            this.conversationService = conversationService;
            this.rentalRequestMessageIdentifier = rentalRequestMessageIdentifier;

            Rental rentalRequest = this.rentalRequestService.GetRentalById(rentalRequestId);
            Game game = this.gameRepository.GetGameById(rentalRequest.GameId);
            User clientUser = this.userRepository.GetById(rentalRequest.ClientId);
            User ownerUser = this.userRepository.GetById(rentalRequest.OwnerId);

            OwnerName = ownerUser.Username;
            GameName = game.Name;
            DeliveryAddress = deliveryAddress;
            RequestDates = rentalRequest.StartDate.ToShortDateString() + DateRangeSeparator + rentalRequest.EndDate.ToShortDateString();

            decimal rentalPrice = this.rentalRequestService.GetRentalPrice(rentalRequestId);
            PaidAmount = rentalPrice.ToString();

            int createdPaymentIdentifier = this.cashPaymentService.AddCashPayment(
                new CashPaymentDataTransferObject(NewPaymentPlaceholderId, rentalRequestId, clientUser.Id, ownerUser.Id, rentalPrice));
            this.conversationService.OnCashPaymentSelected(this.rentalRequestMessageIdentifier, createdPaymentIdentifier);
        }
    }
}

// <copyright file="RentalDataTransferObject.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardGames.Src.DTO
{
    public class RentalDataTransferObject
    {
        public RentalDataTransferObject(
            int id,
            int gameId,
            string gameName,
            int clientId,
            string clientName,
            int ownerId,
            string ownerName,
            DateTime startDate,
            DateTime endDate,
            decimal price)
        {
            this.Id = id;
            this.GameId = gameId;
            this.GameName = gameName;
            this.ClientId = clientId;
            this.ClientName = clientName;
            this.OwnerId = ownerId;
            this.OwnerName = ownerName;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Price = price;
        }

        public RentalDataTransferObject()
        {
            this.GameName = string.Empty;
            this.ClientName = string.Empty;
            this.OwnerName = string.Empty;
        }

        public int Id { get; set; }

        public int GameId { get; set; }

        public string GameName { get; set; }

        public int ClientId { get; set; }

        public string ClientName { get; set; }

        public int OwnerId { get; set; }

        public string OwnerName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }
    }
}

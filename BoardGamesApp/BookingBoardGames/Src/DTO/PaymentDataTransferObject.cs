// <copyright file="PaymentDataTransferObject.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;

namespace BookingBoardGames.Data.DTO
{
    public class PaymentDataTransferObject
    {
        public int PaymentId { get; set; }

        public string? DateText { get; set; }

        public string? ProductName { get; set; }

        public string? ReceiverName { get; set; }

        /// <summary>
        /// Gets or sets numeric amount used strictly for service-level total calculations.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets formatted amount string for display.
        /// </summary>
        public string AmountText => $"{this.Amount:C}";

        public string? PaymentMethod { get; set; }

        public string? FilePath { get; set; }
    }
}

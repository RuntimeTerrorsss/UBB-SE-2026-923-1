// <copyright file="ConversationParticipant.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;

namespace BookingBoardGames.Src.Models
{
    public class ConversationParticipant
    {
        public int ConversationId { get; set; }

        public Conversation? Conversation { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public DateTime LastRead { get; set; } = DateTime.MinValue;
    }
}

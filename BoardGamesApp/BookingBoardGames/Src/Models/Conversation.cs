using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingBoardGames.Src.Models
{
    public class Conversation
    {
        public int ConversationId { get; set; }

        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();

        public ICollection<Message> Messages { get; set; } = new List<Message>();

        public int[] ConversationParticipantIds =>
            this.Participants.Select(p => p.UserId).ToArray();

        public Dictionary<int, DateTime> LastReadByUser =>
            this.Participants.ToDictionary(p => p.UserId, p => p.LastRead);
    }
}

using System;
using System.Collections.Generic;

namespace BookingBoardGames.Src.Models.ChatModels
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public List<Message> ConversationMessageList { get; set; }
        public int[] ConversationParticipantIds { get; set; }
        public Dictionary<int, DateTime> LastMessageReadTime { get; set; }
        public Dictionary<int, int> UnreadMessagesCount { get; set; }

        public Conversation(int conversationIdentifier, int[] participants, List<Message> messages, Dictionary<int, DateTime> lastRead)
        {
            ConversationId = conversationIdentifier;
            ConversationParticipantIds = participants;
            ConversationMessageList = messages;
            LastMessageReadTime = lastRead;
        }
    }
}
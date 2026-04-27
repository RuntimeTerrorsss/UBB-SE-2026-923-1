using System;
using System.Collections.Generic;

public class Conversation
{
    public int ConversationId { get; set; }

    public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

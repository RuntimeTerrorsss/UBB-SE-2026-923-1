using System;
using System.Collections.Generic;
public class ConversationParticipant
{
    public int ConversationId { get; set; }
    public int UserId { get; set; }

    public DateTime? LastMessageReadTime { get; set; }
    public int UnreadMessagesCount { get; set; }

    public Conversation? Conversation { get; set; }
    public User? User { get; set; } 
}
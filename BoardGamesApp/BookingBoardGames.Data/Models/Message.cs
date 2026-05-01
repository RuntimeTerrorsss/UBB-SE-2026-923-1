using System;
using System.Net.NetworkInformation;

using System;

public abstract class Message
{
    public int MessageId { get; set; }
    public DateTime MessageSentTime { get; set; }
    public required string MessageContentAsString { get; set; }


    public int ConversationId { get; set; }
    public int MessageSenderId { get; set; }
    public int MessageReceiverId { get; set; }

    public required Conversation Conversation { get; set; }
    public required User Sender { get; set; }
    public required User Receiver { get; set; }
}


public class TextMessage : Message
{
    public required string TextMessageContent { get; set; }
}

public class ImageMessage : Message
{
    public required string MessageImageUrl { get; set; }
}

public class SystemMessage : Message
{
    public required string MessageContent { get; set; }
}

public class RentalRequestMessage : Message
{
    public int RentalRequestId { get; set; }
    public bool IsRequestResolved { get; set; }
    public bool IsRequestAccepted { get; set; }
    public required string RequestContent { get; set; }

    public required Rental RentalRequest { get; set; } 
}

public class CashAgreementMessage : Message
{
    public int CashPaymentId { get; set; }
    public bool IsCashAgreementResolved { get; set; }
    public bool IsCashAgreementAcceptedByBuyer { get; set; }
    public bool IsCashAgreementAcceptedBySeller { get; set; }
    
    public required Payment CashPayment { get; set; }
}

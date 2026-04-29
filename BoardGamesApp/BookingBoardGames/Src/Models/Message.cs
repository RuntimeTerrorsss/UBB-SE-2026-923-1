using System;
using System.Net.NetworkInformation;

using System;

namespace BookingBoardGames.Src.Models;

public abstract class Message
{
    public int MessageId { get; set; }

    public DateTime MessageSentTime { get; set; }

    public string MessageContentAsString { get; set; }

    public int ConversationId { get; set; }

    public int MessageSenderId { get; set; }

    public int MessageReceiverId { get; set; }

    public Conversation Conversation { get; set; }

    public User Sender { get; set; }

    public User Receiver { get; set; }
}


public class TextMessage : Message
{
    public string TextMessageContent { get; set; }
}

public class ImageMessage : Message
{
    public string MessageImageUrl { get; set; }
}

public class SystemMessage : Message
{
    public string MessageContent { get; set; }
}

public class RentalRequestMessage : Message
{
    public int RentalRequestId { get; set; }

    public bool IsRequestResolved { get; set; }

    public bool IsRequestAccepted { get; set; }

    public string RequestContent { get; set; }

    public Rental RentalRequest { get; set; }
}

public class CashAgreementMessage : Message
{
    public int CashPaymentId { get; set; }

    public bool IsCashAgreementResolved { get; set; }

    public bool IsCashAgreementAcceptedByBuyer { get; set; }

    public bool IsCashAgreementAcceptedBySeller { get; set; }

    public Payment CashPayment { get; set; }
}

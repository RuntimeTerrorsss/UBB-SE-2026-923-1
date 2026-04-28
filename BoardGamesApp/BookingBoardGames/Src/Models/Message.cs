using System;
using BookingBoardGames.Src.Enum;

namespace BookingBoardGames.Src.Models;

public abstract class Message
{
    public int MessageId { get; set; }

    public DateTime MessageSentTime { get; set; }

    public string? MessageContentAsString { get; set; }

    public int ConversationId { get; set; }

    public int MessageSenderId { get; set; }

    public int MessageReceiverId { get; set; }

    public Conversation? Conversation { get; set; }

    public User? Sender { get; set; }

    public User? Receiver { get; set; }

    public abstract MessageType TypeOfMessage { get; }
}

public class TextMessage : Message
{
    public string TextMessageContent { get; set; }

    public override MessageType TypeOfMessage => MessageType.MessageText;
}

public class ImageMessage : Message
{
    public string MessageImageUrl { get; set; }

    public override MessageType TypeOfMessage => MessageType.MessageImage;
}

public class SystemMessage : Message
{
    public string MessageContent { get; set; }

    public override MessageType TypeOfMessage => MessageType.MessageSystem;

    public SystemMessage() { }

    public SystemMessage(int messageId, int conversationId, DateTime sentAt, string content)
    {
        MessageId = messageId;
        ConversationId = conversationId;
        MessageSentTime = sentAt;
        MessageContent = content;
        MessageContentAsString = content;
    }
}

public class RentalRequestMessage : Message
{
    public int RentalRequestId { get; set; }

    public bool IsRequestResolved { get; set; }

    public bool IsRequestAccepted { get; set; }

    public string RequestContent { get; set; }

    public Rental RentalRequest { get; set; }

    public override MessageType TypeOfMessage => MessageType.MessageRentalRequest;

    public RentalRequestMessage() { }

    public RentalRequestMessage(
        int messageId, int conversationId, int senderId, int receiverId,
        DateTime sentAt, string content, int requestId, bool isResolved, bool isAccepted)
    {
        MessageId = messageId;
        ConversationId = conversationId;
        MessageSenderId = senderId;
        MessageReceiverId = receiverId;
        MessageSentTime = sentAt;
        RequestContent = content;
        MessageContentAsString = content;
        RentalRequestId = requestId;
        IsRequestResolved = isResolved;
        IsRequestAccepted = isAccepted;
    }
}

public class CashAgreementMessage : Message
{
    public int CashPaymentId { get; set; }

    public bool IsCashAgreementResolved { get; set; }

    public bool IsCashAgreementAcceptedByBuyer { get; set; }

    public bool IsCashAgreementAcceptedBySeller { get; set; }

    public Payment CashPayment { get; set; }

    public override MessageType TypeOfMessage => MessageType.MessageCashAgreement;

    public CashAgreementMessage() { }
   
    public CashAgreementMessage(
        int messageId, int conversationId, int senderId, int receiverId,
        int paymentId, DateTime sentAt, string content)
    {
        MessageId = messageId;
        ConversationId = conversationId;
        MessageSenderId = senderId;
        MessageReceiverId = receiverId;
        CashPaymentId = paymentId;
        MessageSentTime = sentAt;
        MessageContentAsString = content;
    }
}

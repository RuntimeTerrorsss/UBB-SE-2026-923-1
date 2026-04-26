using System;
using BookingBoardGames.Src.Models.ChatModels;
using BookingBoardGames.Src.Enum;

public class ImageMessage : Message
{
    public string MessageImageUrl { get; set; }
    public ImageMessage(int id, int conversationId, int senderId, int receiverId, DateTime sentAt, string imageUrl) : base(id, conversationId, senderId, receiverId, sentAt, "[Image]", MessageType.MessageImage)
    {
        MessageImageUrl = imageUrl;
    }
}
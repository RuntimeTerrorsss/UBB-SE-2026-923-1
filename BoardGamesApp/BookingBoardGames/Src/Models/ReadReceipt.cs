using System;
using BookingBoardGames.Src.DTO;

namespace BookingBoardGames.Src.Models
{
    // Thin wrapper so the repository layer can work with a stable type
    // while the network/service layer uses ReadReceiptDataTransferObject.
    public class ReadReceipt
    {
        public int conversationId { get; init; }
        public int messageReaderId { get; init; }
        public DateTime timeStamp { get; init; }

        public int messageReceiverId { get; init; }

        public ReadReceipt(int conversationId, int messageReaderId, int messageReceiverId,DateTime timeStamp)
        {
            this.conversationId = conversationId;
            this.messageReaderId = messageReaderId;
            this.messageReceiverId = messageReaderId;
            this.timeStamp = timeStamp;
        }

        // Factory — convert from the DTO the network layer sends
        public static ReadReceipt FromDto(ReadReceiptDataTransferObject dto)
            => new(dto.conversationId, dto.readerId, dto.receiverId, dto.receiptTimeStamp);
    }
}

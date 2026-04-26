using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardGames.Src.Models.ChatModels;

namespace BookingBoardGames.Src.DTO
{
    public record ReadReceiptDataTransferObject(
        int conversationId,
        int readerId,
        int receiverId,
        DateTime receiptTimeStamp);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardGames.Src.DTO
{
    public record ReadReceiptDataTransferObject(
        int conversationId,
        int readerId,
        int receiverId,
        DateTime receiptTimeStamp);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardGames.Src.Models.ChatModels
{
    public record ReadReceipt(
        int conversationId,
        int messageReaderId,
        int messageReceiverId,
        DateTime timeStamp);
}

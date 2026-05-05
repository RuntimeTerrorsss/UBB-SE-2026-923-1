using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardGames.Data.DTO;

namespace BookingBoardGames.Src.Services
{
    public interface IConversationNotifier
    {
        void Register(int userId, IConversationService observer);

        void Unregister(int userId);

        void NotifyMessage(IEnumerable<int> participantUserIds, Message message);

        void NotifyMessageUpdate(IEnumerable<int> participantUserIds, Message message);

        void NotifyReadReceipt(IEnumerable<int> participantUserIds, ReadReceiptDTO readReceipt);

        void NotifyNewConversation(Conversation conversation);
    }
}

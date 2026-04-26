using System;
using BookingBoardGames.Src.Services;
using Microsoft.UI.Xaml;

namespace BookingBoardGames.Src.Navigation
{
    public class BookingNavigationArguments
    {
        public int RequestIdentifier { get; set; }
        public string DeliveryAddress { get; set; }
        public int BookingMessageIdentifier { get; set; }
        public ConversationService ConversationService { get; set; }
        public Window CurrentWindow { get; set; }
    }
}
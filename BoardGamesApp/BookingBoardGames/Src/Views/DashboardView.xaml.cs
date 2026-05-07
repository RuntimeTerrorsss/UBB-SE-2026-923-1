// <copyright file="DashboardView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BookingBoardGames.Data.Services;
using BookingBoardGames.Data.Shared;
using BookingBoardGames.Data.Views.ChatViews;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BookingBoardGames.Data.Views
{
    public sealed partial class DashboardView : Page
    {
        public DashboardView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int userId)
            {
                SessionContext.GetInstance().UserId = userId;
            }
        }

        private void PaymentHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame?.Navigate(typeof(PaymentHistoryView));
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            int currentUserId = SessionContext.GetInstance().UserId;
            if (App.ConversationRepository is { } conversationRepository && App.UserRepository is { } userRepository)
            {
                var conversationService = new ConversationService(conversationRepository, currentUserId, userRepository);
                conversationService.CreateConversation(currentUserId, 1);
            }

            var window1 = new Window();
            var frame1 = new Frame();
            window1.Content = frame1;
            window1.Title = "User " + currentUserId;
            frame1.Navigate(typeof(ChatPageView), currentUserId);
            window1.Activate();
        }

        private void SeeEmptyChat_Click(object sender, RoutedEventArgs e)
        {
            var window1 = new Window();
            var frame1 = new Frame();
            window1.Content = frame1;
            frame1.Navigate(typeof(ChatPageView), ((App)Application.Current).NoChatsUser);
            window1.Activate();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DiscoveryView));
        }
    }
}

// <copyright file="DashboardView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BookingBoardGames.Src.Services;
using BookingBoardGames.Src.Views.ChatViews;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BookingBoardGames.Src.Views
{
    public sealed partial class DashboardView : Page
    {
        public DashboardView()
        {
            this.InitializeComponent();
        }

        private void PaymentHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame?.Navigate(typeof(PaymentHistoryView));
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.ConversationRepository is { } conversationRepository && App.UserRepository is { } userRepository)
            {
                var conversationService = new ConversationService(conversationRepository, 3, userRepository);
                conversationService.CreateConversation(3, 1);
            }

            var window1 = new Window();
            var frame1 = new Frame();
            window1.Content = frame1;
            window1.Title = "Carol";
            frame1.Navigate(typeof(ChatPageView), 3);
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
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }
    }
}

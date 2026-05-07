// <copyright file="ChatPageView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BookingBoardGames.Data.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.OnlineId;
using System.Threading.Tasks;

namespace BookingBoardGames.Data.Views.ChatViews
{
    public sealed partial class ChatPageView : Page
    {
        private ChatPageViewModel chatPageViewModel;
        private int currentUserId;

        public ChatPageView()
        {
            this.InitializeComponent();
            BookingBoardGames.Src.Shared.SessionContext.GetInstance().OnUserChanged += () =>
            {
                var newUserId = BookingBoardGames.Data.Shared.SessionContext.GetInstance().UserId;
                if (this.currentUserId != newUserId)
                {
                    this.currentUserId = newUserId;
                    await this.InitializeAsync(this.currentUserId);
                }
            };
        }

        public async Task InitializeAsync(int currentUserId)
        {
            this.chatPageViewModel = new ChatPageViewModel(currentUserId);
            this.LeftPanel.ViewModel = this.chatPageViewModel.LeftPanelModelView;
            this.RightPanel.ChatViewModel = this.chatPageViewModel.ChatModelView;
            await this.chatPageViewModel.InitializeAsync();
            this.RightPanel.CurrentUserId = currentUserId;
            this.RightPanel.ProceedToPaymentRequested += this.ProceedToPaymentClick;

            this.chatPageViewModel.LeftPanelModelView.PropertyChanged += (sender, propertyChangedEventArgs) =>
            {
                if (propertyChangedEventArgs.PropertyName != nameof(LeftPanelViewModel.SelectedConversation))
                {
                    return;
                }

                this.RightPanel.IsConversationSelected = this.chatPageViewModel.LeftPanelModelView.SelectedConversation != null;
            };
        }

        private async Task AutoSelectConversationWithUser(int otherUserId)
        {
            var conversations = await this.chatPageViewModel.ConversationService.FetchConversations();
            var existing = conversations.FirstOrDefault(c =>
                c.Participants.Any(p => p.UserId == otherUserId));

            if (existing != null)
            {
                var preview = this.chatPageViewModel.LeftPanelModelView.Conversations
                    .FirstOrDefault(c => c.ConversationId == existing.Id);
                if (preview != null)
                {
                    this.chatPageViewModel.LeftPanelModelView.SelectedConversation = preview;
                }
            }
            else
            {
                await this.chatPageViewModel.ConversationService.CreateConversation(this.currentUserId, otherUserId);

                var updatedConversations = await this.chatPageViewModel.ConversationService.FetchConversations();
                var newConversation = updatedConversations.FirstOrDefault(c =>
                    c.Participants.Any(p => p.UserId == otherUserId));

                if (newConversation != null)
                {
                    var preview = this.chatPageViewModel.LeftPanelModelView.Conversations
                        .FirstOrDefault(c => c.ConversationId == newConversation.Id);
                    if (preview != null)
                    {
                        this.chatPageViewModel.LeftPanelModelView.SelectedConversation = preview;
                    }
                }
            }
        }

        private void ProceedToPaymentClick(object sender, (int UserId, int RequestId, int MessageId) paymentArguments)
        {
            var deliveryWindow = new Window();
            var deliveryFrame = new Frame();
            deliveryWindow.Content = deliveryFrame;
            deliveryFrame.Navigate(typeof(DeliveryView), (paymentArguments.UserId, paymentArguments.RequestId, paymentArguments.MessageId, this.chatPageViewModel.ConversationService, deliveryWindow));
            deliveryWindow.Activate();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is ValueTuple<int, int> tuple)
            {
                this.currentUserId = tuple.Item1;
                await this.InitializeAsync(this.currentUserId);
                await this.AutoSelectConversationWithUser(tuple.Item2);
            }
            else if (navigationEventArgs.Parameter is int userId)
            {
                this.currentUserId = userId;
                await this.InitializeAsync(this.currentUserId);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DiscoveryView));
        }
    }
}

// <copyright file="ChatPageView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BookingBoardGames.Src.ViewModels;
using BookingBoardGames.Src.Views;
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

namespace BookingBoardgamesILoveBan.Src.Chat.View
{
    public sealed partial class ChatPageView : Page
    {
        private ChatPageViewModel chatPageViewModel;
        private int currentUserId;

        public ChatPageView()
        {
            InitializeComponent();
        }

        public void Initialize(int currentUserId)
        {
            chatPageViewModel = new ChatPageViewModel(currentUserId);
            LeftPanel.ViewModel = chatPageViewModel.LeftPanelModelView;
            RightPanel.ChatViewModel = chatPageViewModel.ChatModelView;
            RightPanel.CurrentUserId = currentUserId;
            RightPanel.ProceedToPaymentRequested += ProceedToPaymentClick;

            chatPageViewModel.LeftPanelModelView.PropertyChanged += (sender, propertyChangedEventArgs) =>
            {
                if (propertyChangedEventArgs.PropertyName != nameof(LeftPanelViewModel.SelectedConversation))
                {
                    return;
                }
                RightPanel.IsConversationSelected = chatPageViewModel.LeftPanelModelView.SelectedConversation != null;
            };
        }

        private void ProceedToPaymentClick(object sender, (int userId, int requestId, int messageId) paymentArguments)
        {
            var deliveryWindow = new Window();
            var deliveryFrame = new Frame();
            deliveryWindow.Content = deliveryFrame;
            deliveryFrame.Navigate(typeof(DeliveryView), (paymentArguments.userId, paymentArguments.requestId, paymentArguments.messageId, chatPageViewModel.ConversationService, deliveryWindow));
            deliveryWindow.Activate();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);
            currentUserId = (int)navigationEventArgs.Parameter;

            Initialize(currentUserId);
        }
    }
}

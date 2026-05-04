// <copyright file="ChatPageView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BookingBoardGames.Src.ViewModels;
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

namespace BookingBoardGames.Src.Views.ChatViews
{
    public sealed partial class ChatPageView : Page
    {
        private ChatPageViewModel chatPageViewModel;
        private int currentUserId;

        public ChatPageView()
        {
            this.InitializeComponent();
        }

        public void Initialize(int currentUserId)
        {
            this.chatPageViewModel = new ChatPageViewModel(currentUserId);
            this.LeftPanel.ViewModel = this.chatPageViewModel.LeftPanelModelView;
            this.RightPanel.ChatViewModel = this.chatPageViewModel.ChatModelView;
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

        private void ProceedToPaymentClick(object sender, (int UserId, int RequestId, int MessageId) paymentArguments)
        {
            var deliveryWindow = new Window();
            var deliveryFrame = new Frame();
            deliveryWindow.Content = deliveryFrame;
            deliveryFrame.Navigate(typeof(DeliveryView), (paymentArguments.UserId, paymentArguments.RequestId, paymentArguments.MessageId, this.chatPageViewModel.ConversationService, deliveryWindow));
            deliveryWindow.Activate();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);
            this.currentUserId = (int)navigationEventArgs.Parameter;

            this.Initialize(this.currentUserId);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DiscoveryView));
        }
    }
}

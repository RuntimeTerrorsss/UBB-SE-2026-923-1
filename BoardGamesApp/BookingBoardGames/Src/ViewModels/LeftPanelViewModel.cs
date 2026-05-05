// <copyright file="LeftPanelViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookingBoardGames.Data.DTO;
using BookingBoardGames.Data.Repositories;
using Microsoft.UI.Xaml;

namespace BookingBoardGames.Src.ViewModels
{
    public class LeftPanelViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsEmptyStateVisible => this.allConversations.Count == 0;

        public bool IsNoMatchesVisible => this.allConversations.Count > 0 && this.Conversations.Count == 0;

        public bool IsListVisible => this.Conversations.Count > 0;

        private void RefreshUIStates()
        {
            this.OnPropertyChanged(nameof(this.IsEmptyStateVisible));
            this.OnPropertyChanged(nameof(this.IsNoMatchesVisible));
            this.OnPropertyChanged(nameof(this.IsListVisible));
        }

        private List<ConversationPreviewModel> allConversations = new();

        private ObservableCollection<ConversationPreviewModel> conversations;

        public ObservableCollection<ConversationPreviewModel> Conversations
        {
            get => this.conversations;
            set
            {
                this.conversations = value;
                this.OnPropertyChanged();
            }
        }

        private string searchText = string.Empty;

        public string SearchText
        {
            get => this.searchText;
            set
            {
                if (this.searchText != value)
                {
                    this.searchText = value;
                    this.OnPropertyChanged();
                    this.ApplyFilter();
                }
            }
        }

        private int? selectedConversationId;

        public ConversationPreviewModel SelectedConversation
        {
            get => this.Conversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == this.selectedConversationId);
            set
            {
                if (this.selectedConversationId != value?.ConversationId)
                {
                    this.selectedConversationId = value?.ConversationId;

                    if (this.selectedConversationId.HasValue)
                    {
                        this.MarkAsRead(this.selectedConversationId.Value);
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        public LeftPanelViewModel()
        {
            this.Conversations = new ObservableCollection<ConversationPreviewModel>();
        }

        private void ApplyFilter()
        {
            var filteredConversations = this.allConversations
                .Where(conversationItem => string.IsNullOrEmpty(this.SearchText) ||
                            conversationItem.DisplayName.Contains(this.SearchText, StringComparison.Ordinal))
                .ToList();

            for (int i = this.Conversations.Count - 1; i >= 0; i--)
            {
                if (!filteredConversations.Contains(this.Conversations[i]))
                {
                    this.Conversations.RemoveAt(i);
                }
            }

            int notFoundIndex = -1;

            for (int i = 0; i < filteredConversations.Count; i++)
            {
                var filterItem = filteredConversations[i];
                int currentIndex = this.Conversations.IndexOf(filterItem);

                if (currentIndex == notFoundIndex)
                {
                    this.Conversations.Insert(i, filterItem);
                }
                else if (currentIndex != i)
                {
                    this.Conversations.Move(currentIndex, i);
                }
            }

            this.OnPropertyChanged(nameof(this.SelectedConversation));
            this.RefreshUIStates();
        }

        private void MarkAsRead(int conversationId)
        {
            int noUnreadMessagesCount = 0;
            var matchedConversation = this.allConversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == conversationId);
            if (matchedConversation == null || matchedConversation.UnreadCount == noUnreadMessagesCount)
            {
                return;
            }

            matchedConversation.UnreadCount = noUnreadMessagesCount;
        }

        public void HandleIncomingMessage(MessageDataTransferObject message, string senderName)
        {
            this.HandleIncomingMessage(message, senderName, App.UserRepository);
        }

        public void HandleIncomingMessage(MessageDataTransferObject message, string senderName, IUserRepository userService)
        {
            int firstCharacterIndex = 0;
            int singleCharacterLength = 1;
            int noUnreadMessagesCount = 0;
            int singleUnreadMessageCount = 1;

            var matchedConversation = this.allConversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == message.ConversationId);

            if (matchedConversation != null)
            {
                matchedConversation.LastMessageText = message.Content;
                matchedConversation.Timestamp = DateTime.Now;
                matchedConversation.UnreadCount = message.ConversationId == this.selectedConversationId ? noUnreadMessagesCount : matchedConversation.UnreadCount + singleUnreadMessageCount;

                this.allConversations.Remove(matchedConversation);
                this.allConversations.Insert(0, matchedConversation);
            }
            else
            {
                var newConversationPreview = new ConversationPreviewModel(
                    message.ConversationId,
                    senderName,
                    senderName.Substring(firstCharacterIndex, singleCharacterLength).ToUpper(),
                    message.Content,
                    DateTime.Now,
                    unreadCountInput: message.ConversationId == this.selectedConversationId ? noUnreadMessagesCount : singleUnreadMessageCount,
                    userService.GetById(message.ReceiverId).AvatarUrl);
                this.allConversations.Insert(0, newConversationPreview);
            }

            this.ApplyFilter();
        }

        public void HandleIncomingConversation(ConversationDTO conversation, string displayName, int userId)
        {
            this.HandleIncomingConversation(conversation, displayName, userId, App.UserRepository);
        }

        public void HandleIncomingConversation(ConversationDTO conversation, string displayName, int userId, IUserRepository service)
        {
            int firstCharacterIndex = 0;
            int singleCharacterLength = 1;

            var matchedConversation = this.allConversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == conversation.Id);
            if (matchedConversation != null)
            {
                return;
            }

            var otherUserIdentifier = conversation.Participants.First(participantItem => participantItem.UserId != userId).UserId;

            var newConversationPreview = new ConversationPreviewModel(
                conversation.Id,
                displayName,
                displayName.Substring(firstCharacterIndex, singleCharacterLength).ToUpper(),
                conversation.MessageList.LastOrDefault()?.GetChatMessagePreview() ?? string.Empty,
                conversation.MessageList.LastOrDefault()?.SentAt ?? DateTime.MinValue,
                unreadCountInput: conversation.UnreadCount[userId],
                service.GetById(otherUserIdentifier).AvatarUrl);

            this.allConversations.Insert(0, newConversationPreview);
            this.SortConversationsByTimestamp();
            this.ApplyFilter();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void SortConversationsByTimestamp()
        {
            this.allConversations = this.allConversations.OrderByDescending(conversationItem => conversationItem.Timestamp).ToList();
            Debug.WriteLine("sorted conversations:");
            this.ApplyFilter();
        }

        public void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

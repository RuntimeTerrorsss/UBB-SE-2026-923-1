// <copyright file="ChatPageViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;

namespace BookingBoardGames.Src.ViewModels;

public class ChatPageViewModel
{
    private readonly int currentUserId;
    private readonly ConversationService conversationService;
    private readonly List<ConversationDTO> conversations = new();

    public ChatPageViewModel(int currentUser)
   : this(currentUser, new ConversationService(App.ConversationRepository, currentUser))
    {
    }

    public ChatPageViewModel(int currentUser, ConversationService service)
        : this(currentUser, service, App.UserRepository)
    {
    }

    public ChatPageViewModel(int currentUser, ConversationService service, IUserRepository userRepository)
    {
        this.LeftPanelModelView = new LeftPanelViewModel();
        this.ChatModelView = new ChatViewModel(currentUser);
        this.currentUserId = currentUser;

        this.LeftPanelModelView.PropertyChanged += this.OnLeftPanelPropertyChanged;
        this.ChatModelView.MessageSent += this.OnMessageSent;
        this.ChatModelView.BookingRequestUpdate += this.UpdateBookingRequest;
        this.ChatModelView.CashAgreementAccept += this.UpdateCashAgreement;

        this.conversationService = service;
        this.conversations = this.conversationService.FetchConversations();

        foreach (var conversationItem in this.conversations)
        {
            this.LeftPanelModelView.HandleIncomingConversation(
                conversationItem,
                this.conversationService.GetOtherUserNameByConversationDTO(conversationItem),
                this.currentUserId,
                userRepository);
        }

        this.conversationService.ActionMessageProcessed += this.OnMessageReceived;
        this.conversationService.ActionConversationProcessed += this.OnConversationReceived;
        this.conversationService.ActionReadReceiptProcessed += this.OnReadReceiptReceived;
        this.conversationService.ActionMessageUpdateProcessed += this.OnMessageUpdateReceived;
    }

    public LeftPanelViewModel LeftPanelModelView { get; }

    public ChatViewModel ChatModelView { get; }

    public ConversationService ConversationService
    {
        get => this.conversationService;
    }

    private void OnLeftPanelPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
        if (propertyChangedEventArgs.PropertyName != nameof(LeftPanelViewModel.SelectedConversation))
        {
            return;
        }

        if (this.LeftPanelModelView.SelectedConversation == null)
        {
            return;
        }

        var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == this.LeftPanelModelView.SelectedConversation.ConversationId);
        if (matchedConversation == null)
        {
            return;
        }

        int selectedConversationOtherUserUnreadCount = matchedConversation.UnreadCount.FirstOrDefault(unreadItem => unreadItem.Key != this.currentUserId).Value;
        this.ChatModelView.LoadConversation(this.LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, selectedConversationOtherUserUnreadCount);

        this.SendReadReceipt(matchedConversation);
    }

    private void OnMessageSent(MessageDataTransferObject message)
    {
        var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == message.ConversationId);
        int receiverUserId = matchedConversation.Participants.First(participantItem => participantItem.UserId != message.SenderId).UserId;
        message = message with { ReceiverId = receiverUserId };
        this.conversationService.SendMessage(message);
    }

    private void SendReadReceipt(ConversationDTO conversation)
    {
        this.conversationService.SendReadReceipt(conversation);
    }

    private void OnSendMessageUpdate(MessageDataTransferObject message)
    {
        this.conversationService.UpdateMessage(message);
    }

    private void OnMessageReceived(MessageDataTransferObject message, string senderName)
    {
        var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == message.ConversationId);

        matchedConversation?.AddMessageToListDTO(message);

        this.LeftPanelModelView.HandleIncomingMessage(message, senderName);
        this.ChatModelView.HandleIncomingMessage(message);
        if (this.ChatModelView.ConversationId == message.ConversationId)
        {
            this.SendReadReceipt(matchedConversation);
        }
    }

    private void UpdateBookingRequest(int messageId, int conversationId, bool accepted, bool resolved)
    {
        var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationId);
        var targetMessage = matchedConversation?.MessageList.FirstOrDefault(messageItem => messageItem.Id == messageId);
        if (targetMessage == null)
        {
            return;
        }

        targetMessage = targetMessage with { IsResolved = resolved, IsAccepted = accepted };
        this.OnSendMessageUpdate(targetMessage);
    }

    private void UpdateCashAgreement(int messageId, int conversationId)
    {
        var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationId);
        var targetMessage = matchedConversation?.MessageList.FirstOrDefault(messageItem => messageItem.Id == messageId);
        if (targetMessage == null)
        {
            return;
        }

        if (this.currentUserId == targetMessage.SenderId)
        {
            targetMessage = targetMessage with { IsAcceptedBySeller = true };
        }

        if (this.currentUserId == targetMessage.ReceiverId)
        {
            targetMessage = targetMessage with { IsAcceptedByBuyer = true };
        }

        this.OnSendMessageUpdate(targetMessage);
    }

    private void OnConversationReceived(ConversationDTO conversation, string otherUsername)
    {
        this.conversations.Add(conversation);
        this.LeftPanelModelView.HandleIncomingConversation(conversation, otherUsername, this.currentUserId);
    }

    private void OnReadReceiptReceived(ReadReceiptDTO readReceipt)
    {
        var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == readReceipt.ConversationId);
        matchedConversation.LastRead[readReceipt.ReaderId] = readReceipt.ReceiptTimeStamp;
        matchedConversation.UpdateUnreadCounts();
        if (this.ChatModelView.ConversationId == readReceipt.ConversationId && readReceipt.ReaderId != this.currentUserId)
        {
            this.ChatModelView.LoadConversation(this.LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, matchedConversation.UnreadCount[readReceipt.ReaderId]);
        }
    }

    private void OnMessageUpdateReceived(MessageDataTransferObject updatedMessage, string senderName)
    {
        int noUnreadMessagesCount = 0;
        var matchedConversation = this.conversations.FirstOrDefault(conversationItem => conversationItem.Id == updatedMessage.ConversationId);
        if (matchedConversation == null)
        {
            return;
        }

        for (int i = 0; i < matchedConversation.MessageList.Count; i++)
        {
            if (matchedConversation.MessageList[i].Id == updatedMessage.Id)
            {
                matchedConversation.MessageList[i] = updatedMessage;
                if (this.ChatModelView.ConversationId == updatedMessage.ConversationId)
                {
                    this.ChatModelView.LoadConversation(this.LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, noUnreadMessagesCount);
                }

                break;
            }
        }
    }
}

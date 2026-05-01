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
    public LeftPanelViewModel LeftPanelModelView { get; }

    public ChatViewModel ChatModelView { get; }

    private readonly int currentUserId;
    private readonly ConversationService conversationService;

    public ConversationService ConversationService
    {
        get => conversationService;
    }

    private List<ConversationDTO> conversations = new();

    public ChatPageViewModel(int currentUser)
    : this(currentUser, new ConversationService(App.ConversationRepository, currentUser))
    {
    }

    public ChatPageViewModel(int currentUser, ConversationService service) : this(currentUser, service, App.UserRepository)
    {
    }

    public ChatPageViewModel(int currentUser, ConversationService service, IUserRepository userRepository)
    {
        LeftPanelModelView = new LeftPanelViewModel();
        ChatModelView = new ChatViewModel(currentUser);
        currentUserId = currentUser;

        LeftPanelModelView.PropertyChanged += OnLeftPanelPropertyChanged;
        ChatModelView.MessageSent += OnMessageSent;
        ChatModelView.BookingRequestUpdate += UpdateBookingRequest;
        ChatModelView.CashAgreementAccept += UpdateCashAgreement;

        conversationService = service;
        conversations = conversationService.FetchConversations();

        foreach (var conversationItem in conversations)
        {
            LeftPanelModelView.HandleIncomingConversation(
                conversationItem,
                conversationService.GetOtherUserNameByConversationDTO(conversationItem),
                currentUserId,
                userRepository);
        }

        conversationService.ActionMessageProcessed += OnMessageReceived;
        conversationService.ActionConversationProcessed += OnConversationReceived;
        conversationService.ActionReadReceiptProcessed += OnReadReceiptReceived;
        conversationService.ActionMessageUpdateProcessed += OnMessageUpdateReceived;
    }

    private void OnLeftPanelPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
        if (propertyChangedEventArgs.PropertyName != nameof(LeftPanelViewModel.SelectedConversation))
        {
            return;
        }
        if (LeftPanelModelView.SelectedConversation == null)
        {
            return;
        }

        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == LeftPanelModelView.SelectedConversation.ConversationId);
        if (matchedConversation == null)
        {
            return;
        }

        int selectedConversationOtherUserUnreadCount = matchedConversation.UnreadCount.FirstOrDefault(unreadItem => unreadItem.Key != currentUserId).Value;
        ChatModelView.LoadConversation(LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, selectedConversationOtherUserUnreadCount);

        SendReadReceipt(matchedConversation);
    }

    private void OnMessageSent(MessageDataTransferObject message)
    {
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == message.ConversationId);
        int receiverUserId = matchedConversation.Participants.First(participantItem => participantItem.UserId != message.SenderId).UserId;
        message = message with { ReceiverId = receiverUserId };
        conversationService.SendMessage(message);
    }

    private void SendReadReceipt(ConversationDTO conversation)
    {
        conversationService.SendReadReceipt(conversation);
    }

    private void OnSendMessageUpdate(MessageDataTransferObject message)
    {
        conversationService.UpdateMessage(message);
    }

    private void OnMessageReceived(MessageDataTransferObject message, string senderName)
    {
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == message.ConversationId);

        matchedConversation?.AddMessageToListDTO(message);

        LeftPanelModelView.HandleIncomingMessage(message, senderName);
        ChatModelView.HandleIncomingMessage(message);
        if (ChatModelView.ConversationId == message.ConversationId)
        {
            SendReadReceipt(matchedConversation);
        }
    }

    private void UpdateBookingRequest(int messageId, int conversationId, bool accepted, bool resolved)
    {
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationId);
        var targetMessage = matchedConversation?.MessageList.FirstOrDefault(messageItem => messageItem.Id == messageId);
        if (targetMessage == null)
        {
            return;
        }
        targetMessage = targetMessage with { IsResolved = resolved, IsAccepted = accepted };
        OnSendMessageUpdate(targetMessage);
    }

    private void UpdateCashAgreement(int messageId, int conversationId)
    {
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationId);
        var targetMessage = matchedConversation?.MessageList.FirstOrDefault(messageItem => messageItem.Id == messageId);
        if (targetMessage == null)
        {
            return;
        }
        if (currentUserId == targetMessage.SenderId)
        {
            targetMessage = targetMessage with { IsAcceptedBySeller = true };
        }
        if (currentUserId == targetMessage.ReceiverId)
        {
            targetMessage = targetMessage with { IsAcceptedByBuyer = true };
        }
        OnSendMessageUpdate(targetMessage);
    }

    private void OnConversationReceived(ConversationDTO conversation, string otherUsername)
    {
        conversations.Add(conversation);
        LeftPanelModelView.HandleIncomingConversation(conversation, otherUsername, currentUserId);
    }

    private void OnReadReceiptReceived(ReadReceiptDTO readReceipt)
    {
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == readReceipt.ConversationId);
        matchedConversation.LastRead[readReceipt.ReaderId] = readReceipt.ReceiptTimeStamp;
        matchedConversation.UpdateUnreadCounts();
        if (ChatModelView.ConversationId == readReceipt.ConversationId && readReceipt.ReaderId != currentUserId)
        {
            ChatModelView.LoadConversation(LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, matchedConversation.UnreadCount[readReceipt.ReaderId]);
        }
    }

    private void OnMessageUpdateReceived(MessageDataTransferObject updatedMessage, string senderName)
    {
        int noUnreadMessagesCount = 0;
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == updatedMessage.ConversationId);
        if (matchedConversation == null)
        {
            return;
        }
        for (int i = 0; i < matchedConversation.MessageList.Count; i++)
        {
            if (matchedConversation.MessageList[i].Id == updatedMessage.Id)
            {
                matchedConversation.MessageList[i] = updatedMessage;
                if (ChatModelView.ConversationId == updatedMessage.ConversationId)
                {
                    ChatModelView.LoadConversation(LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, noUnreadMessagesCount);
                }
                break;
            }
        }
    }
}

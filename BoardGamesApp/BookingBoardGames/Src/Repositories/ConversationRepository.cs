using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using BookingBoardGames.Data;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Enum;
using BookingBoardGames.Data.DTO;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGames.Data.Interfaces
{
    public class ConversationRepository : IConversationRepository
    {
        public List<Conversation> GetConversationsForUser(int userId)
        {
            return App.Client.GetFromJsonAsync<List<Conversation>>($"conversation/user/{userId}").GetAwaiter().GetResult() ?? new List<Conversation>();
        }

        public Conversation GetConversationById(int conversationId)
        {
            var result = App.Client.GetFromJsonAsync<Conversation>($"conversation/{conversationId}").GetAwaiter().GetResult();
            if (result == null) throw new InvalidOperationException($"Conversation {conversationId} not found.");
            return result;
        }

        public IReadOnlyList<int> GetParticipantUserIds(int conversationId)
        {
            return App.Client.GetFromJsonAsync<List<int>>($"conversation/{conversationId}/participants").GetAwaiter().GetResult() ?? new List<int>();
        }

        public int CreateConversation(int senderId, int receiverId)
        {
            var response = App.Client.PostAsJsonAsync("conversation", new { SenderId = senderId, ReceiverId = receiverId }).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode) return -1;

            var conversation = response.Content.ReadFromJsonAsync<Conversation>().GetAwaiter().GetResult();
            return conversation?.ConversationId ?? -1;
        }

        public Message HandleNewMessage(Message message)
        {
            var dto = MapToMessageDto(message);
            var response = App.Client.PostAsJsonAsync("conversation/messages", dto).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                var persistedDto = response.Content.ReadFromJsonAsync<MessageDataTransferObject>().GetAwaiter().GetResult();
                if (persistedDto != null) return MapDtoToEntity(persistedDto);
            }
            return message;
        }

        public Message? HandleMessageUpdate(Message message)
        {
            var dto = MapToMessageDto(message);
            var response = App.Client.PutAsJsonAsync("conversation/messages", dto).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                var persistedDto = response.Content.ReadFromJsonAsync<MessageDataTransferObject>().GetAwaiter().GetResult();
                if (persistedDto != null) return MapDtoToEntity(persistedDto);
            }
            return null;
        }

        public void HandleReadReceipt(ReadReceiptDTO readReceipt)
        {
            App.Client.PostAsJsonAsync("conversation/readreceipt", readReceipt).GetAwaiter().GetResult();
        }

        public Message? HandleRentalRequestFinalization(int messageId)
        {
            var response = App.Client.PostAsync($"conversation/rental/finalize/{messageId}", null).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var dto = response.Content.ReadFromJsonAsync<MessageDataTransferObject>().GetAwaiter().GetResult();
                if (dto != null) return MapDtoToEntity(dto);
            }
            return null;
        }

        public Message? CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            var response = App.Client.PostAsync($"conversation/cash/{messageIdOfParentRentalRequestMessage}/{paymentId}", null).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var dto = response.Content.ReadFromJsonAsync<MessageDataTransferObject>().GetAwaiter().GetResult();
                if (dto != null) return MapDtoToEntity(dto);
            }
            return null;
        }

        private MessageDataTransferObject MapToMessageDto(Message m)
        {
            var type = m switch { TextMessage => MessageType.MessageText, ImageMessage => MessageType.MessageImage, RentalRequestMessage => MessageType.MessageRentalRequest, CashAgreementMessage => MessageType.MessageCashAgreement, SystemMessage => MessageType.MessageSystem, _ => throw new ArgumentException() };
            var content = m switch { TextMessage t => t.TextMessageContent, RentalRequestMessage r => r.RequestContent, SystemMessage s => s.MessageContent, _ => m.MessageContentAsString } ?? string.Empty;
            return new MessageDataTransferObject(m.MessageId, m.ConversationId, m.MessageSenderId, m.MessageReceiverId, m.MessageSentTime, content, type, m is ImageMessage i ? i.MessageImageUrl ?? "" : "", m is RentalRequestMessage rr ? rr.IsRequestResolved : m is CashAgreementMessage ca && ca.IsCashAgreementResolved, m is RentalRequestMessage ra && ra.IsRequestAccepted, m is CashAgreementMessage cab && cab.IsCashAgreementAcceptedByBuyer, m is CashAgreementMessage cas && cas.IsCashAgreementAcceptedBySeller, m is CashAgreementMessage cap ? cap.CashPaymentId : -1, m is RentalRequestMessage rar ? rar.RentalRequestId : -1);
        }

        private Message MapDtoToEntity(MessageDataTransferObject dto)
        {
            Message msg = dto.Type switch
            {
                MessageType.MessageText => new TextMessage { TextMessageContent = dto.Content, Conversation = null!, Sender = null!, Receiver = null! },
                MessageType.MessageImage => new ImageMessage { MessageImageUrl = dto.ImageUrl, Conversation = null!, Sender = null!, Receiver = null! },
                MessageType.MessageRentalRequest => new RentalRequestMessage { RentalRequestId = dto.RequestId, IsRequestResolved = dto.IsResolved, IsRequestAccepted = dto.IsAccepted, RequestContent = dto.Content, Conversation = null!, Sender = null!, Receiver = null! },
                MessageType.MessageCashAgreement => new CashAgreementMessage { CashPaymentId = dto.PaymentId, IsCashAgreementResolved = dto.IsResolved, IsCashAgreementAcceptedByBuyer = dto.IsAcceptedByBuyer, IsCashAgreementAcceptedBySeller = dto.IsAcceptedBySeller, Conversation = null!, Sender = null!, Receiver = null! },
                MessageType.MessageSystem => new SystemMessage { MessageContent = dto.Content, Conversation = null!, Sender = null!, Receiver = null! },
                _ => throw new ArgumentOutOfRangeException()
            };
            msg.MessageId = dto.Id; msg.ConversationId = dto.ConversationId; msg.MessageSenderId = dto.SenderId; msg.MessageReceiverId = dto.ReceiverId; msg.MessageSentTime = dto.SentAt; msg.MessageContentAsString = dto.Content;
            return msg;
        }
    }
}
